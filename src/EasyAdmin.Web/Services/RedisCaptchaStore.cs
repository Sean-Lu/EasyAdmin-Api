using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Web.Contracts;
using Sean.Core.Redis;

namespace EasyAdmin.Web.Services;

/// <summary>
/// Redis验证码存储
/// </summary>
public class RedisCaptchaStore : ICaptchaStore
{
    private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 保存验证码
    /// </summary>
    public async Task StoreAsync(string captchaKey, string value, TimeSpan expiration)
    {
        var stored = await RedisHelper.StringSetAsync(GetCacheKey(captchaKey), value, expiration);
        if (!stored)
        {
            throw new InvalidOperationException("Failed to store captcha challenge");
        }
    }

    /// <summary>
    /// 消费验证码
    /// </summary>
    public async Task<string?> ConsumeAsync(string captchaKey)
    {
        var lockKey = $"{CacheKeyConst.CaptchaLockPrefix}{captchaKey}";
        var lockToken = Guid.NewGuid().ToString("N");
        if (!await RedisHelper.LockTakeAsync(lockKey, lockToken, LockExpiration))
        {
            return null;
        }

        try
        {
            var cacheKey = GetCacheKey(captchaKey);
            var value = await RedisHelper.StringGetAsync<string>(cacheKey);
            await RedisHelper.KeyDeleteAsync(cacheKey);
            return value;
        }
        finally
        {
            await RedisHelper.LockReleaseAsync(lockKey, lockToken);
        }
    }

    private static string GetCacheKey(string captchaKey) => $"{CacheKeyConst.CaptchaPrefix}{captchaKey}";
}
