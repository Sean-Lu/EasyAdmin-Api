using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 数据库连接配置列表查询 DTO
/// </summary>
public class DbConnectionConfigListReqDto : PageRequestBase
{
    /// <summary>
    /// 数据库连接配置名称
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public CodeGenDbType? DbType { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public CommonState? State { get; set; }
}