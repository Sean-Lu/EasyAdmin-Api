using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 字典表
/// </summary>
//[Table("SysDict")]
[CodeFirst]
public class SysDictEntity : TreeEntityBase<SysDictEntity>
{
    /// <summary>
    /// 编码
    /// </summary>
    [MaxLength(50)]
    public virtual string Code { get; set; }
    /// <summary>
    /// 字段键
    /// </summary>
    public virtual int DictKey { get; set; }
    /// <summary>
    /// 字段值
    /// </summary>
    [MaxLength(200)]
    public virtual string DictValue { get; set; }
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