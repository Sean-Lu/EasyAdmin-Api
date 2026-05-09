namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 代码生成请求 DTO
/// </summary>
public class CodeGenReqDto
{
    /// <summary>
    /// 数据库连接配置ID
    /// </summary>
    public long DbConfigId { get; set; }
    /// <summary>
    /// 表名列表
    /// </summary>
    public List<string> TableNames { get; set; }
    /// <summary>
    /// 模板ID列表
    /// </summary>
    public List<long> TemplateIds { get; set; }
    /// <summary>
    /// 包名
    /// </summary>
    public string PackageName { get; set; }
    /// <summary>
    /// 模块名
    /// </summary>
    public string ModuleName { get; set; }
    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; }
    /// <summary>
    /// 表前缀
    /// </summary>
    public string TablePrefix { get; set; }
}