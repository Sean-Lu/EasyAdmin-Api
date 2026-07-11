using EasyAdmin.Application.Contracts;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Infrastructure.Const;
using Microsoft.Extensions.Logging;
using Sean.Core.Redis;

namespace EasyAdmin.Application.Services;

public class AuthPasswordVerifier(
    ILogger<AuthPasswordVerifier> logger,
    IUserService userService)
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan FailedAttemptWindow = TimeSpan.FromMinutes(1);

    public async Task<bool> VerifyAsync(long userId, string? password, string ipAddress = "")
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(password))
        {
            throw new ExplicitException("密码不能为空");
        }

        var key = $"{CacheKeyConst.LockPasswordFailedPrefix}{userId}:{ipAddress}";
        var failedCount = await RedisHelper.StringGetAsync<long>(key);
        if (failedCount >= MaxFailedAttempts)
        {
            throw new ExplicitException("操作过于频繁，请稍后重试");
        }

        if (!await userService.CheckPasswordAsync(userId, password))
        {
            var next = await RedisHelper.StringIncrementAsync(key);
            if (next == 1) await RedisHelper.KeyExpireAsync(key, FailedAttemptWindow);
            logger.LogWarning("用户 {UserId} 锁屏密码验证失败", userId);
            if (next >= MaxFailedAttempts) throw new ExplicitException("操作过于频繁，请稍后重试");
            throw new ExplicitException("密码错误");
        }

        await RedisHelper.KeyDeleteAsync(key);
        return true;
    }
}
