using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// AI租户设置表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(TenantId) }, "UX_AiTenantSetting_TenantId", DbIndexType.Unique)]
public class AiTenantSettingEntity : TenantEntityBase
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 每日请求上限
    /// </summary>
    public int DailyRequestLimit { get; set; }
}