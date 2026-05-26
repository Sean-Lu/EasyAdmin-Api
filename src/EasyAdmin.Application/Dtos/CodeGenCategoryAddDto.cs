namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 代码生成模板分类新增 DTO
/// </summary>
public class CodeGenCategoryAddDto
{
    public string Name { get; set; }
    public string Code { get; set; }
    public int SortOrder { get; set; } = 0;
    public string? Description { get; set; }
}