using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 数据库连接配置表
/// </summary>
[CodeFirst]
public class DbConnectionConfigEntity : EntityBase
{
    /// <summary>
    /// 配置名称
    /// </summary>
    [MaxLength(50)]
    [Description("配置名称")]
    public virtual string Name { get; set; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    [Description("数据库类型")]
    public virtual CodeGenDbType DbType { get; set; }
    /// <summary>
    /// 数据库主机
    /// </summary>
    [MaxLength(100)]
    [Description("数据库主机")]
    public virtual string Host { get; set; }
    /// <summary>
    /// 数据库端口
    /// </summary>
    [Description("数据库端口")]
    public virtual int Port { get; set; }
    /// <summary>
    /// 数据库名
    /// </summary>
    [MaxLength(100)]
    [Description("数据库名")]
    public virtual string Database { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(50)]
    [Description("用户名")]
    public virtual string Username { get; set; }
    /// <summary>
    /// 密码（加密存储）
    /// </summary>
    [MaxLength(200)]
    [Description("密码")]
    public virtual string Password { get; set; }
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    [MaxLength(500)]
    [Description("数据库连接字符串")]
    public virtual string? ConnectionString { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    [Description("状态")]
    public virtual CommonState State { get; set; }
}