using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 数据库连接配置新增/更新 DTO
/// </summary>
public class DbConnectionConfigUpdateDto : DtoIdBase
{
    /// <summary>
    /// 数据库连接配置名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public CodeGenDbType DbType { get; set; }
    /// <summary>
    /// 数据库主机
    /// </summary>
    public string Host { get; set; }
    /// <summary>
    /// 数据库端口
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// 数据库名称
    /// </summary>
    public string Database { get; set; }
    /// <summary>
    /// 数据库用户名
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 数据库密码
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public string? ConnectionString { get; set; }
}