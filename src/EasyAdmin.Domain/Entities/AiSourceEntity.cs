using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// AI引用来源表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(TenantId), nameof(ConversationId), nameof(MessageId) }, "IX_AiSource_ConversationMessage")]
public class AiSourceEntity : TenantEntityBase
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
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// 来源日期
    /// </summary>
    public DateTime? SourceDate { get; set; }

    /// <summary>
    /// 摘录
    /// </summary>
    [MaxLength(500)]
    public string? Excerpt { get; set; }

    /// <summary>
    /// 打开路径
    /// </summary>
    [MaxLength(500)]
    public string? Route { get; set; }
}
