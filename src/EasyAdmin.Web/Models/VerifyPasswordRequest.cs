namespace EasyAdmin.Web.Models;

/// <summary>
/// 验证密码请求
/// </summary>
public class VerifyPasswordRequest
{
    /// <summary>
    /// 当前密码
    /// </summary>
    public string? Password { get; set; }
}
