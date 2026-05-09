using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 代码生成模板列表查询 DTO
/// </summary>
public class CodeGenTemplateListReqDto : PageRequestBase
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 模板类型
    /// </summary>
    public CodeGenTemplateType? TemplateType { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public CommonState? State { get; set; }
    /// <summary>
    /// 分类ID
    /// </summary>
    public long? CategoryId { get; set; }
}