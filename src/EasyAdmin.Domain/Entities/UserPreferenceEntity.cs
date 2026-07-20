using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 用户偏好表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(TenantId), nameof(CreateUserId), nameof(PreferenceKey) }, "UX_UserPreference_OwnerKey", DbIndexType.Unique)]
public class UserPreferenceEntity : TenantEntityBase
{
    /// <summary>
    /// 偏好键
    /// </summary>
    [MaxLength(100)]
    public virtual string PreferenceKey { get; set; } = string.Empty;

    /// <summary>
    /// 偏好值
    /// </summary>
    [MaxLength(5000)]
    public virtual string PreferenceValue { get; set; } = string.Empty;
}
