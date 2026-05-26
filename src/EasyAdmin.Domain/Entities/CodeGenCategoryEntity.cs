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
    [MaxLength(50)]
    [Description("分类名称")]
    public virtual string Name { get; set; }
    [MaxLength(50)]
    [Description("分类编码")]
    public virtual string Code { get; set; }
    [Description("排序号")]
    public virtual int SortOrder { get; set; }
    [MaxLength(500)]
    [Description("分类描述")]
    public virtual string? Description { get; set; }
    [Description("是否内置分类")]
    public virtual bool IsBuiltIn { get; set; }
    [Description("状态")]
    public virtual CommonState State { get; set; }
}