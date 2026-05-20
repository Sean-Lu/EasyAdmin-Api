namespace EasyAdmin.Application.Dtos;

public class CodeFirstParseReqDto
{
    public string SourceCode { get; set; }
    public string Language { get; set; }
}

public class CodeFirstParseResultDto
{
    public string ClassName { get; set; }
    public string TableName { get; set; }
    public string TableComment { get; set; }
    public string Namespace { get; set; }
    public List<CodeGenColumnConfigDto> Columns { get; set; }
}