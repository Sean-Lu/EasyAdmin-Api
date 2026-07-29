using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EasyAdmin.Infrastructure.Ai;

namespace EasyAdmin.Test;

[TestClass]
public class OpenAiCompatibleClientTests
{
    [TestMethod]
    public async Task StreamAsync_ParsesSplitTextToolUsageAndDoneEvents()
    {
        var sse = """
                  : keepalive

                  data: {"choices":[{"delta":{"content":"Hel"}}]}

                  data: {"choices":[{"delta":{"content":"lo","tool_calls":[{"index":0,"id":"call-1","function":{"name":"lookup_notes","arguments":"{\"key"}}]}}]}

                  data: {"choices":[{"delta":{"tool_calls":[{"index":0,"function":{"arguments":"word\":\"AI\"}"}}]},"finish_reason":"tool_calls"}],"usage":{"prompt_tokens":7,"completion_tokens":3,"total_tokens":10}}

                  data: [DONE]

                  """;
        var handler = new RecordingHandler(_ => CreateSseResponse(sse, 1, 2, 5, 3, 8));
        var client = new OpenAiCompatibleClient(new HttpClient(handler));
        var request = CreateRequest([
            new AiToolDefinition(
                "lookup_notes",
                "Look up notes",
                JsonDocument.Parse("""{"type":"object"}""").RootElement.Clone())
        ]);

        var events = await CollectAsync(client.StreamAsync(CreateOptions(), request, CancellationToken.None));

        Assert.AreEqual(new Uri("https://example.test/v1/chat/completions"), handler.RequestUri);
        Assert.AreEqual("Bearer", handler.Authorization?.Scheme);
        Assert.AreEqual("sk-test", handler.Authorization?.Parameter);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.AreEqual("test-model", body.RootElement.GetProperty("model").GetString());
        Assert.IsTrue(body.RootElement.GetProperty("stream").GetBoolean());
        Assert.IsTrue(body.RootElement.GetProperty("stream_options").GetProperty("include_usage").GetBoolean());
        Assert.AreEqual("auto", body.RootElement.GetProperty("tool_choice").GetString());

        CollectionAssert.AreEqual(
            new[] { "Hel", "lo" },
            events.OfType<AiModelTextDelta>().Select(item => item.Text).ToArray());
        var toolEvent = events.OfType<AiModelToolCallsCompleted>().Single();
        Assert.AreEqual(1, toolEvent.ToolCalls.Count);
        Assert.AreEqual("call-1", toolEvent.ToolCalls[0].Id);
        Assert.AreEqual("lookup_notes", toolEvent.ToolCalls[0].Name);
        Assert.AreEqual("""{"keyword":"AI"}""", toolEvent.ToolCalls[0].ArgumentsJson);
        var usage = events.OfType<AiModelUsageCompleted>().Single();
        Assert.AreEqual(7, usage.InputTokens);
        Assert.AreEqual(3, usage.OutputTokens);
        Assert.AreEqual(10, usage.TotalTokens);
        Assert.AreEqual(1, events.OfType<AiModelCompleted>().Count());
    }

    [TestMethod]
    [DataRow(HttpStatusCode.Unauthorized, "ai_authentication")]
    [DataRow(HttpStatusCode.Forbidden, "ai_authentication")]
    [DataRow(HttpStatusCode.TooManyRequests, "ai_rate_limited")]
    [DataRow(HttpStatusCode.BadGateway, "ai_provider_error")]
    public async Task StreamAsync_MapsHttpFailuresWithoutLeakingResponseBody(
        HttpStatusCode statusCode,
        string expectedErrorType)
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("provider-secret-body")
        });
        var client = new OpenAiCompatibleClient(new HttpClient(handler));

        var exception = await Assert.ThrowsExactlyAsync<AiModelClientException>(
            () => CollectAsync(client.StreamAsync(CreateOptions(), CreateRequest(), CancellationToken.None)));

        Assert.AreEqual(expectedErrorType, exception.ErrorType);
        Assert.IsFalse(exception.Message.Contains("provider-secret-body", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task StreamAsync_MapsTimeout()
    {
        var handler = new RecordingHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            return CreateSseResponse("data: [DONE]\n\n");
        });
        var client = new OpenAiCompatibleClient(new HttpClient(handler));
        var options = CreateOptions();
        options.Timeout = TimeSpan.FromMilliseconds(25);

        var exception = await Assert.ThrowsExactlyAsync<AiModelClientException>(
            () => CollectAsync(client.StreamAsync(options, CreateRequest(), CancellationToken.None)));

        Assert.AreEqual("ai_timeout", exception.ErrorType);
    }

    [TestMethod]
    public async Task StreamAsync_OmitsToolFieldsWhenRequestHasNoTools()
    {
        var handler = new RecordingHandler(_ => CreateSseResponse(
            "data: {\"choices\":[{\"delta\":{\"content\":\"ok\"}}]}\n\ndata: [DONE]\n\n"));
        var client = new OpenAiCompatibleClient(new HttpClient(handler));

        await CollectAsync(client.StreamAsync(CreateOptions(), CreateRequest(), CancellationToken.None));

        using var body = JsonDocument.Parse(handler.Body!);
        Assert.IsFalse(body.RootElement.TryGetProperty("tools", out _));
        Assert.IsFalse(body.RootElement.TryGetProperty("tool_choice", out _));
        var requestMessage = body.RootElement.GetProperty("messages")[0];
        Assert.IsFalse(requestMessage.TryGetProperty("tool_call_id", out _));
        Assert.IsFalse(requestMessage.TryGetProperty("tool_calls", out _));
    }

    [TestMethod]
    public async Task StreamAsync_MapsCallerCancellation()
    {
        var handler = new RecordingHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            return CreateSseResponse("data: [DONE]\n\n");
        });
        var client = new OpenAiCompatibleClient(new HttpClient(handler));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(25));

        var exception = await Assert.ThrowsExactlyAsync<AiModelClientException>(
            () => CollectAsync(client.StreamAsync(CreateOptions(), CreateRequest(), cancellation.Token)));

        Assert.AreEqual("ai_cancelled", exception.ErrorType);
    }

    [TestMethod]
    public async Task StreamAsync_RejectsMalformedJson()
    {
        var handler = new RecordingHandler(_ => CreateSseResponse("data: {not-json}\n\ndata: [DONE]\n\n"));
        var client = new OpenAiCompatibleClient(new HttpClient(handler));

        var exception = await Assert.ThrowsExactlyAsync<AiModelClientException>(
            () => CollectAsync(client.StreamAsync(CreateOptions(), CreateRequest(), CancellationToken.None)));

        Assert.AreEqual("ai_invalid_response", exception.ErrorType);
    }

    [TestMethod]
    public async Task StreamAsync_RejectsUnknownToolName()
    {
        var handler = new RecordingHandler(_ => CreateSseResponse(
            "data: {\"choices\":[{\"delta\":{\"tool_calls\":[{\"index\":0,\"id\":\"call-1\",\"function\":{\"name\":\"delete_all\",\"arguments\":\"{}\"}}]},\"finish_reason\":\"tool_calls\"}]}\n\ndata: [DONE]\n\n"));
        var client = new OpenAiCompatibleClient(new HttpClient(handler));

        var exception = await Assert.ThrowsExactlyAsync<AiModelClientException>(
            () => CollectAsync(client.StreamAsync(CreateOptions(), CreateRequest(), CancellationToken.None)));

        Assert.AreEqual("ai_invalid_response", exception.ErrorType);
    }

    [TestMethod]
    public async Task StreamAsync_RejectsToolArgumentsLargerThan16Kb()
    {
        var arguments = new string('x', 16 * 1024 + 1);
        var payload = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    delta = new
                    {
                        tool_calls = new[]
                        {
                            new
                            {
                                index = 0,
                                id = "call-1",
                                function = new { name = "lookup_notes", arguments }
                            }
                        }
                    },
                    finish_reason = "tool_calls"
                }
            }
        });
        var handler = new RecordingHandler(_ => CreateSseResponse($"data: {payload}\n\ndata: [DONE]\n\n"));
        var client = new OpenAiCompatibleClient(new HttpClient(handler));
        var request = CreateRequest([
            new AiToolDefinition(
                "lookup_notes",
                "Look up notes",
                JsonDocument.Parse("""{"type":"object"}""").RootElement.Clone())
        ]);

        var exception = await Assert.ThrowsExactlyAsync<AiModelClientException>(
            () => CollectAsync(client.StreamAsync(CreateOptions(), request, CancellationToken.None)));

        Assert.AreEqual("ai_invalid_response", exception.ErrorType);
    }

    private static AiModelClientOptions CreateOptions()
    {
        return new AiModelClientOptions
        {
            BaseUrl = "https://example.test/v1/",
            ApiKey = "sk-test",
            Model = "test-model",
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private static AiModelRequest CreateRequest(IReadOnlyList<AiToolDefinition>? tools = null)
    {
        return new AiModelRequest
        {
            Messages = [new AiModelMessage("user", "hello")],
            Tools = tools ?? [],
            Temperature = 0.2m,
            MaxOutputTokens = 128
        };
    }

    private static async Task<List<AiModelStreamEvent>> CollectAsync(
        IAsyncEnumerable<AiModelStreamEvent> events)
    {
        var result = new List<AiModelStreamEvent>();
        await foreach (var item in events)
        {
            result.Add(item);
        }
        return result;
    }

    private static HttpResponseMessage CreateSseResponse(string value, params int[] chunkSizes)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var bytes = Encoding.UTF8.GetBytes(value);
        response.Content = new StreamContent(new ChunkedReadStream(bytes, chunkSizes));
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/event-stream");
        return response;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _response;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> response)
            : this((request, _) => Task.FromResult(response(request)))
        {
        }

        public RecordingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> response)
        {
            _response = response;
        }

        public Uri? RequestUri { get; private set; }
        public AuthenticationHeaderValue? Authorization { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            Authorization = request.Headers.Authorization;
            Body = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return await _response(request, cancellationToken);
        }
    }

    private sealed class ChunkedReadStream(byte[] bytes, int[] chunkSizes) : Stream
    {
        private int _position;
        private int _chunkIndex;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => bytes.Length;
        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadCore(buffer.AsSpan(offset, count));
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ReadCore(buffer.Span));
        }

        private int ReadCore(Span<byte> buffer)
        {
            if (_position >= bytes.Length)
            {
                return 0;
            }

            var requested = chunkSizes.Length == 0
                ? buffer.Length
                : chunkSizes[_chunkIndex++ % chunkSizes.Length];
            var count = Math.Min(Math.Min(requested, buffer.Length), bytes.Length - _position);
            bytes.AsSpan(_position, count).CopyTo(buffer);
            _position += count;
            return count;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
