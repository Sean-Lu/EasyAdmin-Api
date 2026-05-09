namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 数据库表信息 DTO
/// </summary>
public class DbTableInfoDto
{
    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; }
    /// <summary>
    /// 表注释
    /// </summary>
    public string TableComment { get; set; }
    /// <summary>
    /// 列信息
    /// </summary>
    public List<DbColumnInfoDto> Columns { get; set; }
}