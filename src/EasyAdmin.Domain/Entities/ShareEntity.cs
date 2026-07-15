using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 分享表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(ShareCode) }, "UX_Share_ShareCode", DbIndexType.Unique)]
[Index(new[] { nameof(TenantId), nameof(CreateUserId), nameof(TargetType), nameof(TargetId) }, "UX_Share_OwnerTarget", DbIndexType.Unique)]
public class ShareEntity : TenantEntityBase
{
    /// <summary>
    /// 分享码
    /// </summary>
    [Required]
    [MaxLength(64)]
    public virtual string ShareCode { get; set; } = string.Empty;
    /// <summary>
    /// 目标类型
    /// </summary>
    public virtual ShareTargetType TargetType { get; set; }
    /// <summary>
    /// 目标ID
    /// </summary>
    public virtual long TargetId { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    public virtual DateTime? ExpiresAt { get; set; }
    /// <summary>
    /// 密码密文
    /// </summary>
    [MaxLength(1024)]
    public virtual string? PasswordCiphertext { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    [DefaultValue(false)]
    public virtual bool IsEnabled { get; set; }
    /// <summary>
    /// 访问版本
    /// </summary>
    [DefaultValue(1)]
    public virtual int AccessVersion { get; set; } = 1;
}