namespace EasyAdmin.Application.Dtos;

public class CodeGenCategoryUpdateDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public int SortOrder { get; set; }
    public string Description { get; set; }
    public int State { get; set; }
}