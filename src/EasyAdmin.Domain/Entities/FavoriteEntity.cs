using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 收藏表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(TenantId), nameof(CreateUserId), nameof(TargetType), nameof(SourceType), nameof(TargetId) }, "UX_Favorite_OwnerTarget", DbIndexType.Unique)]
public class FavoriteEntity : TenantEntityBase
{
    /// <summary>
    /// 目标类型
    /// </summary>
    public virtual FavoriteTargetType TargetType { get; set; }
    /// <summary>
    /// 来源类型
    /// </summary>
    public virtual FavoriteSourceType SourceType { get; set; }
    /// <summary>
    /// 目标ID
    /// </summary>
    public virtual long TargetId { get; set; }
    /// <summary>
    /// 标题快照
    /// </summary>
    [MaxLength(200)]
    public virtual string? TitleSnapshot { get; set; }
    /// <summary>
    /// 分享者快照
    /// </summary>
    [MaxLength(100)]
    public virtual string? OwnerNameSnapshot { get; set; }
}
