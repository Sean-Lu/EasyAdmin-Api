using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Web.Models;

/// <summary>
/// 登录请求模型
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 账号（用户名 / 手机号 / 邮箱）
    /// </summary>
    public required string Account { get; set; }
    /// <summary>
    /// 密码（LoginType=Password 时必填）
    /// </summary>
    public string? Password { get; set; }
    /// <summary>
    /// 登录方式（默认 Password）
    /// </summary>
    public LoginType LoginType { get; set; } = LoginType.Password;
    /// <summary>
    /// 验证码标识
    /// </summary>
    public string? CaptchaKey { get; set; }
    /// <summary>
    /// 验证码
    /// </summary>
    public string? CaptchaCode { get; set; }
}