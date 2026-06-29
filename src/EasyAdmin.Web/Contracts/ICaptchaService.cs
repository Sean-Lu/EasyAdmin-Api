using EasyAdmin.Web.Models;

namespace EasyAdmin.Web.Contracts;

/// <summary>
/// 验证码服务
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// 是否启用
    /// </summary>
    bool Enabled { get; }
    /// <summary>
    /// 生成验证码
    /// </summary>
    Task<CaptchaResponse> GenerateAsync();
    /// <summary>
    /// 校验验证码
    /// </summary>
    Task<bool> ValidateAsync(string? captchaKey, string? captchaCode);
}

/// <summary>
/// 验证码生成器
/// </summary>
public interface ICaptchaCodeGenerator
{
    /// <summary>
    /// 生成验证码
    /// </summary>
    CaptchaCode Generate(int codeLength);
}

/// <summary>
/// 验证码存储
/// </summary>
public interface ICaptchaStore
{
    /// <summary>
    /// 保存验证码
    /// </summary>
    Task StoreAsync(string captchaKey, string value, TimeSpan expiration);
    /// <summary>
    /// 消费验证码
    /// </summary>
    Task<string?> ConsumeAsync(string captchaKey);
}
