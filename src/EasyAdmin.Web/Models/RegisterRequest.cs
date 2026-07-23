namespace EasyAdmin.Web.Models;

/// <summary>
/// 用户注册请求
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// 租户编码
    /// </summary>
    public string? TenantCode { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public required string UserName { get; set; }
    /// <summary>
    /// 密码(MD5)
    /// </summary>
    public required string Password { get; set; }
    /// <summary>
    /// 手机号码
    /// </summary>
    public string? PhoneNumber { get; set; }
    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string? Email { get; set; }
    /// <summary>
    /// 验证码标识
    /// </summary>
    public string? CaptchaKey { get; set; }
    /// <summary>
    /// 验证码
    /// </summary>
    public string? CaptchaCode { get; set; }
}