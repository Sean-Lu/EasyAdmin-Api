namespace EasyAdmin.Application.Dtos;

public class CodeGenCategoryAddDto
{
    public string Name { get; set; }
    public string Code { get; set; }
    public int SortOrder { get; set; } = 0;
    public string? Description { get; set; }
}