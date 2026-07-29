using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace EasyAdmin.Infrastructure.Ai;

/// <summary>
/// OpenAI兼容模型客户端
/// </summary>
public sealed class OpenAiCompatibleClient(HttpClient httpClient) : IAiModelClient
{
    private const int MaxToolArgumentsBytes = 16 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async IAsyncEnumerable<AiModelStreamEvent> StreamAsync(
        AiModelClientOptions options,
        AiModelRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(options.Timeout);
        using var requestMessage = CreateRequest(options, request);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            throw CreateCancellationException(cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new AiModelClientException("ai_provider_error");
        }

        using (response)
        {
            ThrowForStatus(response.StatusCode);

            IAsyncEnumerator<AiModelStreamEvent> enumerator;
            try
            {
                var stream = await response.Content.ReadAsStreamAsync(timeoutSource.Token);
                enumerator = ParseEventsAsync(stream, request.Tools, timeoutSource.Token)
                    .GetAsyncEnumerator(timeoutSource.Token);
            }
            catch (OperationCanceledException)
            {
                throw CreateCancellationException(cancellationToken);
            }
            catch (Exception exception) when (exception is not AiModelClientException)
            {
                throw new AiModelClientException("ai_invalid_response");
            }

            await using (enumerator)
            {
                while (true)
                {
                    AiModelStreamEvent current;
                    try
                    {
                        if (!await enumerator.MoveNextAsync())
                        {
                            break;
                        }
                        current = enumerator.Current;
                    }
                    catch (OperationCanceledException)
                    {
                        throw CreateCancellationException(cancellationToken);
                    }
                    catch (AiModelClientException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        throw new AiModelClientException("ai_invalid_response");
                    }

                    yield return current;
                }
            }
        }
    }

    private static HttpRequestMessage CreateRequest(
        AiModelClientOptions options,
        AiModelRequest request)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"{options.BaseUrl.TrimEnd('/')}/chat/completions");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        var payload = new Dictionary<string, object?>
        {
            ["model"] = options.Model,
            ["messages"] = request.Messages.Select(ToRequestMessage),
            ["stream"] = true,
            ["stream_options"] = new { include_usage = true },
            ["temperature"] = request.Temperature,
            ["max_tokens"] = request.MaxOutputTokens
        };
        if (request.Tools.Count > 0)
        {
            payload["tools"] = request.Tools.Select(item => new
            {
                type = "function",
                function = new
                {
                    name = item.Name,
                    description = item.Description,
                    parameters = item.Parameters
                }
            });
            payload["tool_choice"] = "auto";
        }
        message.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");
        return message;
    }

    private static object ToRequestMessage(AiModelMessage message)
    {
        var result = new Dictionary<string, object?>
        {
            ["role"] = message.Role,
            ["content"] = message.Content
        };
        if (!string.IsNullOrWhiteSpace(message.ToolCallId))
        {
            result["tool_call_id"] = message.ToolCallId;
        }
        if (message.ToolCalls is { Count: > 0 })
        {
            result["tool_calls"] = message.ToolCalls.Select(item => new
            {
                id = item.Id,
                type = "function",
                function = new
                {
                    name = item.Name,
                    arguments = item.ArgumentsJson
                }
            });
        }
        return result;
    }

    private static async IAsyncEnumerable<AiModelStreamEvent> ParseEventsAsync(
        Stream stream,
        IReadOnlyList<AiToolDefinition> tools,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: false);
        var dataLines = new List<string>();
        var calls = new Dictionary<int, ToolCallBuilder>();
        var allowedTools = tools.Select(item => item.Name).ToHashSet(StringComparer.Ordinal);
        var completed = false;

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null)
            {
                break;
            }

            if (line.Length == 0)
            {
                foreach (var item in ParseDataEvent(dataLines, calls, allowedTools, ref completed))
                {
                    yield return item;
                }
                dataLines.Clear();
                continue;
            }

            if (line.StartsWith(':'))
            {
                continue;
            }

            if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                var value = line[5..];
                dataLines.Add(value.StartsWith(' ') ? value[1..] : value);
            }
        }

        if (dataLines.Count > 0)
        {
            foreach (var item in ParseDataEvent(dataLines, calls, allowedTools, ref completed))
            {
                yield return item;
            }
        }

        if (!completed)
        {
            throw new AiModelClientException("ai_invalid_response");
        }
    }

    private static IReadOnlyList<AiModelStreamEvent> ParseDataEvent(
        IReadOnlyList<string> dataLines,
        Dictionary<int, ToolCallBuilder> calls,
        HashSet<string> allowedTools,
        ref bool completed)
    {
        if (dataLines.Count == 0)
        {
            return [];
        }

        var data = string.Join('\n', dataLines);
        if (data == "[DONE]")
        {
            var doneEvents = new List<AiModelStreamEvent>();
            AddCompletedToolCalls(calls, allowedTools, doneEvents);
            doneEvents.Add(new AiModelCompleted());
            completed = true;
            return doneEvents;
        }

        try
        {
            using var document = JsonDocument.Parse(data);
            var root = document.RootElement;
            var events = new List<AiModelStreamEvent>();
            if (root.TryGetProperty("choices", out var choices))
            {
                foreach (var choice in choices.EnumerateArray())
                {
                    if (choice.TryGetProperty("delta", out var delta))
                    {
                        if (delta.TryGetProperty("content", out var content) &&
                            content.ValueKind == JsonValueKind.String)
                        {
                            var text = content.GetString();
                            if (!string.IsNullOrEmpty(text))
                            {
                                events.Add(new AiModelTextDelta(text));
                            }
                        }
                        AppendToolCalls(delta, calls);
                    }

                    if (choice.TryGetProperty("finish_reason", out var finishReason) &&
                        finishReason.ValueKind == JsonValueKind.String)
                    {
                        AddCompletedToolCalls(calls, allowedTools, events);
                    }
                }
            }

            if (root.TryGetProperty("usage", out var usage) &&
                usage.ValueKind == JsonValueKind.Object)
            {
                events.Add(new AiModelUsageCompleted(
                    ReadNonnegativeInt(usage, "prompt_tokens"),
                    ReadNonnegativeInt(usage, "completion_tokens"),
                    ReadNonnegativeInt(usage, "total_tokens")));
            }
            return events;
        }
        catch (AiModelClientException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw new AiModelClientException("ai_invalid_response");
        }
        catch (InvalidOperationException)
        {
            throw new AiModelClientException("ai_invalid_response");
        }
    }

    private static void AppendToolCalls(
        JsonElement delta,
        Dictionary<int, ToolCallBuilder> calls)
    {
        if (!delta.TryGetProperty("tool_calls", out var toolCalls) ||
            toolCalls.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var item in toolCalls.EnumerateArray())
        {
            var index = item.GetProperty("index").GetInt32();
            if (index < 0)
            {
                throw new AiModelClientException("ai_invalid_response");
            }

            if (!calls.TryGetValue(index, out var builder))
            {
                builder = new ToolCallBuilder();
                calls.Add(index, builder);
            }

            if (item.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
            {
                builder.Id.Append(id.GetString());
            }

            if (!item.TryGetProperty("function", out var function) ||
                function.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (function.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            {
                builder.Name.Append(name.GetString());
            }
            if (function.TryGetProperty("arguments", out var arguments) &&
                arguments.ValueKind == JsonValueKind.String)
            {
                builder.Arguments.Append(arguments.GetString());
                if (Encoding.UTF8.GetByteCount(builder.Arguments.ToString()) > MaxToolArgumentsBytes)
                {
                    throw new AiModelClientException("ai_invalid_response");
                }
            }
        }
    }

    private static void AddCompletedToolCalls(
        Dictionary<int, ToolCallBuilder> calls,
        HashSet<string> allowedTools,
        ICollection<AiModelStreamEvent> events)
    {
        if (calls.Count == 0)
        {
            return;
        }

        var completedCalls = calls
            .OrderBy(item => item.Key)
            .Select(item =>
            {
                var id = item.Value.Id.ToString();
                var name = item.Value.Name.ToString();
                var arguments = item.Value.Arguments.ToString();
                if (string.IsNullOrWhiteSpace(id) ||
                    string.IsNullOrWhiteSpace(name) ||
                    !allowedTools.Contains(name))
                {
                    throw new AiModelClientException("ai_invalid_response");
                }

                try
                {
                    using var _ = JsonDocument.Parse(arguments);
                }
                catch (JsonException)
                {
                    throw new AiModelClientException("ai_invalid_response");
                }
                return new AiModelToolCall(id, name, arguments);
            })
            .ToList();
        calls.Clear();
        events.Add(new AiModelToolCallsCompleted(completedCalls));
    }

    private static int ReadNonnegativeInt(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) ||
            !value.TryGetInt32(out var result))
        {
            return 0;
        }
        return Math.Max(0, result);
    }

    private static void ThrowForStatus(HttpStatusCode statusCode)
    {
        if ((int)statusCode is >= 200 and < 300)
        {
            return;
        }

        throw new AiModelClientException(statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "ai_authentication",
            HttpStatusCode.TooManyRequests => "ai_rate_limited",
            _ => "ai_provider_error"
        });
    }

    private static AiModelClientException CreateCancellationException(
        CancellationToken callerCancellation)
    {
        return new AiModelClientException(
            callerCancellation.IsCancellationRequested ? "ai_cancelled" : "ai_timeout");
    }

    private sealed class ToolCallBuilder
    {
        /// <summary>
        /// 工具调用ID
        /// </summary>
        public StringBuilder Id { get; } = new();

        /// <summary>
        /// 工具名称
        /// </summary>
        public StringBuilder Name { get; } = new();

        /// <summary>
        /// 工具参数
        /// </summary>
        public StringBuilder Arguments { get; } = new();
    }
}
