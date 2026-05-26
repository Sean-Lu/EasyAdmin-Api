namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 代码生成模板新增 DTO
/// </summary>
public class CodeGenTemplateAddDto
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 模板编码
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// 模板内容
    /// </summary>
    public string Content { get; set; }
    /// <summary>
    /// 模板描述
    /// </summary>
    public string? Description { get; set; }
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
}