using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 字典类型表
/// </summary>
[CodeFirst]
public class SysDictTypeEntity : TenantEntityBase
{
    /// <summary>
    /// 字典类型名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 字典类型编码
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(200)]
    public virtual string? Remark { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}
