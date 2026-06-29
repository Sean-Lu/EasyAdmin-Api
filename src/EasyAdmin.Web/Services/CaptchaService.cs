using System.Security.Cryptography;
using System.Text;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Models;
using Microsoft.Extensions.Options;

namespace EasyAdmin.Web.Services;

/// <summary>
/// 验证码服务
/// </summary>
public class CaptchaService(
    IOptions<CaptchaOptions> options,
    ICaptchaCodeGenerator codeGenerator,
    ICaptchaStore store) : ICaptchaService
{
    private readonly CaptchaOptions _options = options.Value;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled => _options.Enable;

    /// <summary>
    /// 生成验证码
    /// </summary>
    public async Task<CaptchaResponse> GenerateAsync()
    {
        if (!Enabled)
        {
            return new CaptchaResponse { Enabled = false };
        }

        var captchaKey = Guid.NewGuid().ToString("N");
        var captcha = codeGenerator.Generate(_options.CodeLength);

        try
        {
            await store.StoreAsync(
                captchaKey,
                ComputeHash(captchaKey, captcha.Code),
                TimeSpan.FromSeconds(_options.ExpireSeconds));
        }
        catch (Exception exception) when (exception is not ExplicitException)
        {
            throw new ExplicitException("验证码服务暂不可用");
        }

        return new CaptchaResponse
        {
            Enabled = true,
            CaptchaKey = captchaKey,
            Image = captcha.Image
        };
    }

    /// <summary>
    /// 校验验证码
    /// </summary>
    public async Task<bool> ValidateAsync(string? captchaKey, string? captchaCode)
    {
        if (!Enabled)
        {
            return true;
        }

        if (!Guid.TryParseExact(captchaKey, "N", out _))
        {
            return false;
        }

        string? expectedHash;
        try
        {
            expectedHash = await store.ConsumeAsync(captchaKey);
        }
        catch (Exception exception) when (exception is not ExplicitException)
        {
            throw new ExplicitException("验证码服务暂不可用");
        }

        if (string.IsNullOrWhiteSpace(expectedHash) || string.IsNullOrWhiteSpace(captchaCode))
        {
            return false;
        }

        return string.Equals(expectedHash, ComputeHash(captchaKey, captchaCode), StringComparison.Ordinal);
    }

    private static string ComputeHash(string captchaKey, string captchaCode)
    {
        var normalizedCode = captchaCode.Trim().ToUpperInvariant();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{captchaKey}:{normalizedCode}")));
    }
}
