namespace EasyAdmin.Web.Models;

public class LoginResponse
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 刷新令牌（双Token模式返回）
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// AccessToken过期时间（秒）
    /// </summary>
    public long ExpiresIn { get; set; }
}