namespace EasyAdmin.Infrastructure.Models;

/// <summary>
/// SMTP 邮件配置
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// SMTP 服务器地址
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP 服务器端口
    /// </summary>
    public int Port { get; set; } = 25;

    /// <summary>
    /// 是否启用 SSL
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// SMTP 登录用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// SMTP 登录密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 发件邮箱地址
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// 发件人名称
    /// </summary>
    public string FromName { get; set; } = string.Empty;
}
