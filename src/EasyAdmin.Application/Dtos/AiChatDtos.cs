using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// AI聊天请求
/// </summary>
public sealed class AiChatRequestDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public long ConversationId { get; set; }

    /// <summary>
    /// 用户输入
    /// </summary>
    public string Prompt { get; set; } = string.Empty;
}

/// <summary>
/// AI取消请求
/// </summary>
public sealed class AiChatCancelReqDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public long ConversationId { get; set; }
}

/// <summary>
/// AI来源解析请求
/// </summary>
public sealed class AiSourceResolveReqDto
{
    /// <summary>
    /// 来源类型
    /// </summary>
    public AiSourceType SourceType { get; set; }

    /// <summary>
    /// 来源ID
    /// </summary>
    public long SourceId { get; set; }
}

/// <summary>
/// AI来源解析结果
/// </summary>
public sealed class AiSourceResolveDto
{
    /// <summary>
    /// 打开路径
    /// </summary>
    public string Route { get; set; } = string.Empty;
}

/// <summary>
/// AI流事件
/// </summary>
public sealed class AiStreamEventDto
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 事件数据
    /// </summary>
    public object? Data { get; set; }
}

/// <summary>
/// AI引用事件
/// </summary>
public sealed class AiSourceEventDto
{
    /// <summary>
    /// 引用序号
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// 引用来源
    /// </summary>
    public AiSourceCandidate Source { get; set; } = new();
}

/// <summary>
/// AI流式引用
/// </summary>
public sealed class AiStreamSourceDto
{
    /// <summary>
    /// 引用序号
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// 来源类型
    /// </summary>
    public AiSourceType SourceType { get; set; }

    /// <summary>
    /// 来源ID
    /// </summary>
    public long SourceId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 来源日期
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// 内容摘录
    /// </summary>
    public string? Excerpt { get; set; }

    /// <summary>
    /// 打开路径
    /// </summary>
    public string? Route { get; set; }
}
