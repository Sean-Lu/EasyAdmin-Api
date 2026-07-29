using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// AI草稿表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(TenantId), nameof(UserId), nameof(UpdateTime) }, "IX_AiDraft_OwnerUpdate")]
public class AiDraftEntity : TenantEntityBase
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
    /// 草稿类型
    /// </summary>
    public AiDraftType DraftType { get; set; }

    /// <summary>
    /// 内容JSON
    /// </summary>
    [Required]
    [MaxLength(20000)]
    public string ContentJson { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 确认开始时间
    /// </summary>
    public DateTime? ConfirmationStartedAt { get; set; }

    /// <summary>
    /// 确认目标ID
    /// </summary>
    public long? ConfirmedTargetId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public AiDraftStatus Status { get; set; }
}