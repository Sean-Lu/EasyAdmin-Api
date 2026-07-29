using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// AI会话
/// </summary>
public sealed class AiConversationDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 摘要
    /// </summary>
    public string? Summary { get; set; }
}

/// <summary>
/// AI会话分页参数
/// </summary>
public sealed class AiConversationPageReqDto : PageRequestBase
{
    /// <summary>
    /// 关键词
    /// </summary>
    public string? Keyword { get; set; }
}

/// <summary>
/// AI会话重命名参数
/// </summary>
public sealed class AiConversationRenameReqDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// AI会话删除参数
/// </summary>
public sealed class AiConversationDeleteReqDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// AI消息分页参数
/// </summary>
public sealed class AiMessagePageReqDto : PageRequestBase
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public long ConversationId { get; set; }
}

/// <summary>
/// AI消息
/// </summary>
public sealed class AiMessageDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public long ConversationId { get; set; }

    /// <summary>
    /// 顺序
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    public AiMessageRole Role { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public AiMessageStatus Status { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 模型
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 错误类型
    /// </summary>
    public string? ErrorType { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    public List<AiSourceDto> Sources { get; set; } = new();

    /// <summary>
    /// 草稿
    /// </summary>
    public List<AiDraftDto> Drafts { get; set; } = new();
}

/// <summary>
/// AI消息来源
/// </summary>
public sealed class AiSourceDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public long ConversationId { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public long MessageId { get; set; }

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
    public string? Title { get; set; }

    /// <summary>
    /// 摘录
    /// </summary>
    public string? Excerpt { get; set; }

    /// <summary>
    /// 来源日期
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// 打开路径
    /// </summary>
    public string? Route { get; set; }
}
