namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 代码生成模板分类更新 DTO
/// </summary>
public class CodeGenCategoryUpdateDto : DtoIdBase
{
    public string Name { get; set; }
    public string Code { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
}