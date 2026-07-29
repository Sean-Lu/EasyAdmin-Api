using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// AI消息表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(TenantId), nameof(ConversationId), nameof(Sequence) }, "UX_AiMessage_ConversationSequence", DbIndexType.Unique)]
public class AiMessageEntity : TenantEntityBase
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
    [Required]
    [MaxLength(20000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 模型
    /// </summary>
    [MaxLength(100)]
    public string? Model { get; set; }

    /// <summary>
    /// 错误类型
    /// </summary>
    [MaxLength(100)]
    public string? ErrorType { get; set; }
}
