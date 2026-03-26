using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 岗位表
/// </summary>
[CodeFirst]
public class PositionEntity : TenantEntityBase
{
    /// <summary>
    /// 岗位名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 岗位编码
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public virtual int Sort { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(200)]
    public virtual string? Remark { get; set; }
}
