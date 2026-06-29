namespace EasyAdmin.Web.Models;

/// <summary>
/// 验证码响应
/// </summary>
public class CaptchaResponse
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }
    /// <summary>
    /// 验证码标识
    /// </summary>
    public string? CaptchaKey { get; set; }
    /// <summary>
    /// 验证码图片
    /// </summary>
    public string? Image { get; set; }
}

/// <summary>
/// 验证码内容
/// </summary>
public class CaptchaCode(string code, string image)
{
    /// <summary>
    /// 验证码
    /// </summary>
    public string Code { get; } = code;
    /// <summary>
    /// 验证码图片
    /// </summary>
    public string Image { get; } = image;
}
