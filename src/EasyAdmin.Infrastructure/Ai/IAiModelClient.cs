using System.Text.Json;

namespace EasyAdmin.Infrastructure.Ai;

/// <summary>
/// AI模型客户端
/// </summary>
public interface IAiModelClient
{
    /// <summary>
    /// 流式请求模型
    /// </summary>
    IAsyncEnumerable<AiModelStreamEvent> StreamAsync(
        AiModelClientOptions options,
        AiModelRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// AI模型客户端配置
/// </summary>
public sealed class AiModelClientOptions
{
    /// <summary>
    /// 服务地址
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 接口密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 请求超时
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
}

/// <summary>
/// AI模型请求
/// </summary>
public sealed class AiModelRequest
{
    /// <summary>
    /// 消息列表
    /// </summary>
    public IReadOnlyList<AiModelMessage> Messages { get; set; } = [];

    /// <summary>
    /// 工具列表
    /// </summary>
    public IReadOnlyList<AiToolDefinition> Tools { get; set; } = [];

    /// <summary>
    /// 生成温度
    /// </summary>
    public decimal Temperature { get; set; }

    /// <summary>
    /// 最大输出令牌
    /// </summary>
    public int MaxOutputTokens { get; set; }
}

/// <summary>
/// AI模型消息
/// </summary>
/// <param name="Role">消息角色</param>
/// <param name="Content">消息内容</param>
/// <param name="ToolCallId">工具调用ID</param>
/// <param name="ToolCalls">工具调用列表</param>
public sealed record AiModelMessage(
    string Role,
    string? Content,
    string? ToolCallId = null,
    IReadOnlyList<AiModelToolCall>? ToolCalls = null);

/// <summary>
/// AI工具定义
/// </summary>
/// <param name="Name">工具名称</param>
/// <param name="Description">工具说明</param>
/// <param name="Parameters">参数定义</param>
public sealed record AiToolDefinition(
    string Name,
    string Description,
    JsonElement Parameters);

/// <summary>
/// AI工具调用
/// </summary>
/// <param name="Id">工具调用ID</param>
/// <param name="Name">工具名称</param>
/// <param name="ArgumentsJson">参数JSON</param>
public sealed record AiModelToolCall(
    string Id,
    string Name,
    string ArgumentsJson);

/// <summary>
/// AI模型流事件
/// </summary>
public abstract record AiModelStreamEvent;

/// <summary>
/// AI文本增量
/// </summary>
/// <param name="Text">增量文本</param>
public sealed record AiModelTextDelta(string Text) : AiModelStreamEvent;

/// <summary>
/// AI工具调用完成事件
/// </summary>
/// <param name="ToolCalls">工具调用列表</param>
public sealed record AiModelToolCallsCompleted(
    IReadOnlyList<AiModelToolCall> ToolCalls) : AiModelStreamEvent;

/// <summary>
/// AI用量完成事件
/// </summary>
/// <param name="InputTokens">输入令牌数</param>
/// <param name="OutputTokens">输出令牌数</param>
/// <param name="TotalTokens">总令牌数</param>
public sealed record AiModelUsageCompleted(
    int InputTokens,
    int OutputTokens,
    int TotalTokens) : AiModelStreamEvent;

/// <summary>
/// AI生成完成事件
/// </summary>
public sealed record AiModelCompleted : AiModelStreamEvent;

/// <summary>
/// AI模型客户端异常
/// </summary>
public sealed class AiModelClientException(string errorType)
    : Exception("AI provider request failed")
{
    /// <summary>
    /// 错误类型
    /// </summary>
    public string ErrorType { get; } = errorType;
}