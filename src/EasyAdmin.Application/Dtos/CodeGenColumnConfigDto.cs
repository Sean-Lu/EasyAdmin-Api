namespace EasyAdmin.Application.Dtos;

public class CodeGenColumnConfigDto
{
    public string PropertyName { get; set; }
    public string? FieldName { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnComment { get; set; }
    public string? DbType { get; set; }
    public string? CSharpType { get; set; }
    public string? JavaType { get; set; }
    public bool IsNullable { get; set; }
    public bool IsKey { get; set; }
    public bool IsIdentity { get; set; }
}