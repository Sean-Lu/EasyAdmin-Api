namespace EasyAdmin.Web.Models;

/// <summary>
/// 验证码配置
/// </summary>
public class CaptchaOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; }
    /// <summary>
    /// 验证码长度
    /// </summary>
    public int CodeLength { get; set; } = 4;
    /// <summary>
    /// 有效期秒数
    /// </summary>
    public int ExpireSeconds { get; set; } = 120;
}
