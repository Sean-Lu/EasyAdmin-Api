using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Wrapper;
using Sean.Core.Redis;
using System.Security.Cryptography;
using System.Text;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 分享访问令牌
/// </summary>
public sealed record ShareAccessTokenPayload(string ShareCode, int AccessVersion, DateTime ExpiresAt);

/// <summary>
/// 分享安全工具
/// </summary>
public static class ShareSecurity
{
    public const int AccessTokenExpireMinutes = 30;
    public const int MaxFailedAttempts = 5;
    private static readonly TimeSpan FailedAttemptWindow = TimeSpan.FromMinutes(10);

    public static string CreateCode()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }

    public static DateTime CalculateTokenExpiry(DateTime now, DateTime? shareExpiry)
    {
        var tokenExpiry = now.AddMinutes(AccessTokenExpireMinutes);
        return shareExpiry.HasValue && shareExpiry.Value < tokenExpiry ? shareExpiry.Value : tokenExpiry;
    }

    public static bool IsTokenPayloadValid(
        ShareAccessTokenPayload? payload,
        string shareCode,
        int accessVersion,
        DateTime now)
    {
        return payload != null
               && payload.ShareCode == shareCode
               && payload.AccessVersion == accessVersion
               && payload.ExpiresAt > now;
    }

    public static string GetFailureKey(string shareCode, string ipAddress)
    {
        var ipHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ipAddress ?? string.Empty)))
            .ToLowerInvariant();
        return $"{CacheKeyConst.SharePasswordFailedPrefix}{shareCode}:{ipHash}";
    }

    public static async Task EnsurePasswordAttemptAllowedAsync(string shareCode, string ipAddress)
    {
        var failedCount = await RedisHelper.StringGetAsync<long>(GetFailureKey(shareCode, ipAddress));
        if (failedCount >= MaxFailedAttempts)
        {
            throw new ExplicitException("操作过于频繁，请稍后重试");
        }
    }

    public static async Task RegisterPasswordFailureAsync(string shareCode, string ipAddress)
    {
        var key = GetFailureKey(shareCode, ipAddress);
        var failedCount = await RedisHelper.StringIncrementAsync(key);
        if (failedCount == 1)
        {
            await RedisHelper.KeyExpireAsync(key, FailedAttemptWindow);
        }
        if (failedCount >= MaxFailedAttempts)
        {
            throw new ExplicitException("操作过于频繁，请稍后重试");
        }
        throw new ExplicitException("分享密码错误");
    }

    public static Task ClearPasswordFailuresAsync(string shareCode, string ipAddress)
    {
        return RedisHelper.KeyDeleteAsync(GetFailureKey(shareCode, ipAddress));
    }

    public static async Task<(string AccessToken, int ExpireMinutes)> CreateAccessTokenAsync(
        string shareCode,
        int accessVersion,
        DateTime? shareExpiry,
        DateTime now)
    {
        var expiresAt = CalculateTokenExpiry(now, shareExpiry);
        if (expiresAt <= now)
        {
            throw new ExplicitException("分享不存在或已失效");
        }

        var token = CreateCode();
        var payload = new ShareAccessTokenPayload(shareCode, accessVersion, expiresAt);
        await RedisHelper.StringSetAsync(GetAccessTokenKey(token), payload, expiresAt - now);
        return (token, Math.Max(1, (int)Math.Ceiling((expiresAt - now).TotalMinutes)));
    }

    public static async Task<bool> ValidateAccessTokenAsync(
        string? accessToken,
        string shareCode,
        int accessVersion,
        DateTime now)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        var payload = await RedisHelper.StringGetAsync<ShareAccessTokenPayload>(GetAccessTokenKey(accessToken));
        return IsTokenPayloadValid(payload, shareCode, accessVersion, now);
    }

    private static string GetAccessTokenKey(string accessToken)
    {
        return $"{CacheKeyConst.ShareAccessTokenPrefix}{accessToken}";
    }
}