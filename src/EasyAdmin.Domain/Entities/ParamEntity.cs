using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;
using System.ComponentModel;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 参数配置表
/// </summary>
//[Table("Param")]
[CodeFirst]
public class ParamEntity : EntityBase
{
    /// <summary>
    /// 参数名称
    /// </summary>
    [MaxLength(50)]
    public virtual string ParamName { get; set; }
    /// <summary>
    /// 参数键名
    /// </summary>
    [MaxLength(50)]
    public virtual string ParamKey { get; set; }
    /// <summary>
    /// 参数键值
    /// </summary>
    [MaxLength(200)]
    public virtual string ParamValue { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(200)]
    public virtual string Remark { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}