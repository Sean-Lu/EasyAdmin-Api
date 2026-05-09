namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 数据库列信息 DTO
/// </summary>
public class DbColumnInfoDto
{
    /// <summary>
    /// 列名
    /// </summary>
    public string ColumnName { get; set; }
    /// <summary>
    /// 列注释
    /// </summary>
    public string ColumnComment { get; set; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public string DbType { get; set; }
    /// <summary>
    /// C# 类型
    /// </summary>
    public string CSharpType { get; set; }
    /// <summary>
    /// Java 类型
    /// </summary>
    public string JavaType { get; set; }
    /// <summary>
    /// 是否可空
    /// </summary>
    public bool IsNullable { get; set; }
    /// <summary>
    /// 是否主键
    /// </summary>
    public bool IsKey { get; set; }
    /// <summary>
    /// 是否自增
    /// </summary>
    public bool IsIdentity { get; set; }
}