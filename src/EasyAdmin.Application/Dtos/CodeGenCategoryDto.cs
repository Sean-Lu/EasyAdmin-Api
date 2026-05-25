namespace EasyAdmin.Application.Dtos;

public class CodeGenCategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public bool IsBuiltIn { get; set; }
    public int State { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
}