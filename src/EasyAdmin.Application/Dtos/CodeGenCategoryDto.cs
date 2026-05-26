namespace EasyAdmin.Application.Dtos;

public class CodeGenCategoryDto : DtoBase
{
    public string Name { get; set; }
    public string Code { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public bool IsBuiltIn { get; set; }
    public int State { get; set; }
}