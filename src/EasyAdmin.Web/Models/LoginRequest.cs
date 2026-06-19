namespace EasyAdmin.Web.Models;

/// <summary>
/// 登录请求模型
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    public required string Username { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    public required string Password { get; set; }
}