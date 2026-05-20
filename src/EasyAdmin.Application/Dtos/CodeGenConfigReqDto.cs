namespace EasyAdmin.Application.Dtos;

public class CodeGenConfigReqDto
{
    public string ClassName { get; set; }
    public string? InstanceName { get; set; }
    public string? TableName { get; set; }
    public string? TableComment { get; set; }
    public string? PackageName { get; set; }
    public string? ModuleName { get; set; }
    public string? Author { get; set; }
    public List<long> TemplateIds { get; set; }
    public List<CodeGenColumnConfigDto>? Columns { get; set; }
}