using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 代码生成分类表
/// </summary>
[CodeFirst]
public class CodeGenCategoryEntity : EntityBase
{
    /// <summary>
    /// 分类名称
    /// </summary>
    [MaxLength(50)]
    [Description("分类名称")]
    public virtual string Name { get; set; }
    /// <summary>
    /// 分类编码
    /// </summary>
    [MaxLength(50)]
    [Description("分类编码")]
    public virtual string Code { get; set; }
    /// <summary>
    /// 排序号
    /// </summary>
    [Description("排序号")]
    public virtual int SortOrder { get; set; }
    /// <summary>
    /// 分类描述
    /// </summary>
    [MaxLength(500)]
    [Description("分类描述")]
    public virtual string? Description { get; set; }
    /// <summary>
    /// 是否内置分类
    /// </summary>
    [Description("是否内置分类")]
    public virtual bool IsBuiltIn { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    [Description("状态")]
    public virtual CommonState State { get; set; }
}