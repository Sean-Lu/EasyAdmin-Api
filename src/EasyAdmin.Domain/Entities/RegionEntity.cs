using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 行政区划表
/// </summary>
[CodeFirst]
public class RegionEntity : TreeEntityBase<RegionEntity>
{
    /// <summary>
    /// 行政区划名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 行政区划代码
    /// </summary>
    [Required]
    [MaxLength(20)]
    public virtual string Code { get; set; }
    /// <summary>
    /// 层级（1-省，2-市，3-区）
    /// </summary>
    public virtual int Level { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 上级行政区划完整路径
    /// </summary>
    [NotMapped]
    public virtual string? ParentFullPath { get; set; }
}
