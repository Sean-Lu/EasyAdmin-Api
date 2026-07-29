using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// AI调用用量表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(TenantId), nameof(UserId), nameof(UpdateTime) }, "IX_AiUsage_OwnerUpdate")]
public class AiUsageEntity : TenantEntityBase
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
    /// 模型
    /// </summary>
    [MaxLength(100)]
    public string? Model { get; set; }

    /// <summary>
    /// 输入令牌数
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// 输出令牌数
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// 总令牌数
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// 耗时毫秒
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public AiUsageStatus Status { get; set; }

    /// <summary>
    /// 错误类型
    /// </summary>
    [MaxLength(100)]
    public string? ErrorType { get; set; }
}
