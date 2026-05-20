using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 代码生成模板新增/更新 DTO
/// </summary>
public class CodeGenTemplateUpdateDto : DtoIdBase
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 模板代码
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// 模板类型
    /// </summary>
    public CodeGenTemplateType TemplateType { get; set; }
    /// <summary>
    /// 模板描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 分类ID
    /// </summary>
    public long CategoryId { get; set; }
    /// <summary>
    /// 文件路径模板（包含目录和文件名，支持模板变量如 {{ClassName}}）
    /// </summary>
    public string FilePath { get; set; }
    /// <summary>
    /// 是否默认
    /// </summary>
    public bool IsDefault { get; set; }
    /// <summary>
    /// 排序号
    /// </summary>
    public int SortOrder { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public CommonState State { get; set; }
}