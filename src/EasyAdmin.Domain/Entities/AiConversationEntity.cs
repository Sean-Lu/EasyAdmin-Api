using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// AI会话表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(TenantId), nameof(UserId), nameof(UpdateTime) }, "IX_AiConversation_OwnerUpdate")]
public class AiConversationEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 摘要
    /// </summary>
    [MaxLength(4000)]
    public string? Summary { get; set; }

    /// <summary>
    /// 摘要截止消息顺序
    /// </summary>
    public int SummarySequence { get; set; }
}
