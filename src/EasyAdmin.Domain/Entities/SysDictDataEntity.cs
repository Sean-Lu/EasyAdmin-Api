using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 字典数据表
/// </summary>
[CodeFirst]
public class SysDictDataEntity : TenantEntityBase
{
    /// <summary>
    /// 字典类型ID
    /// </summary>
    public virtual long DictTypeId { get; set; }
    /// <summary>
    /// 字典键值
    /// </summary>
    public virtual int DictKey { get; set; }
    /// <summary>
    /// 字典值
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string DictValue { get; set; }
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
