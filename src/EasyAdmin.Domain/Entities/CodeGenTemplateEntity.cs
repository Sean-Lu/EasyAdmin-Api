using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 代码生成模板表
/// </summary>
[CodeFirst]
public class CodeGenTemplateEntity : EntityBase
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [MaxLength(50)]
    [Description("模板名称")]
    public virtual string Name { get; set; }

    /// <summary>
    /// 模板编码
    /// </summary>
    [MaxLength(50)]
    [Description("模板编码")]
    public virtual string Code { get; set; }

    /// <summary>
    /// 模板类型（0-内置，1-用户上传）
    /// </summary>
    [Description("模板类型")]
    public virtual CodeGenTemplateType TemplateType { get; set; }

    /// <summary>
    /// 模板内容
    /// </summary>
    [MaxLength(5000)]
    [Description("模板内容")]
    public virtual string Content { get; set; }

    /// <summary>
    /// 模板描述
    /// </summary>
    [MaxLength(500)]
    [Description("模板描述")]
    public virtual string? Description { get; set; }

    /// <summary>
    /// 分类ID
    /// </summary>
    [Description("分类ID")]
    public virtual long CategoryId { get; set; }

    /// <summary>
    /// 文件路径模板（包含目录和文件名，支持模板变量如 {{ClassName}}）
    /// </summary>
    [MaxLength(200)]
    [Description("文件路径模板")]
    public virtual string FilePath { get; set; }

    /// <summary>
    /// 是否默认模板
    /// </summary>
    [Description("是否默认模板")]
    public virtual bool IsDefault { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    [Description("排序号")]
    public virtual int SortOrder { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Description("状态")]
    public virtual CommonState State { get; set; }
}
