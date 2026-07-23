namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 用户注册参数
/// </summary>
public class RegisterUserDto
{
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
}
