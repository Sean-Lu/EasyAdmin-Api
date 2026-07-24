using EasyAdmin.Application.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Infrastructure.Const;
using Sean.Core.Redis;

namespace EasyAdmin.Application.Services;

public class AuthPasswordVerifier(
    IUserService userService,
    IParamService paramService)
{
    private const int DefaultMaxFailedAttempts = 5;
    private const int DefaultFailedAttemptWindowMinutes = 1;

    public async Task<bool> VerifyAsync(long userId, string? password, string ipAddress = "")
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(password))
        {
            throw new ExplicitException("密码不能为空");
        }

        var user = await userService.GetByIdAsync(userId);
        return await VerifyAsync(user, password, ipAddress);
    }

    public async Task<bool> VerifyAsync(UserEntity user, string? password, string ipAddress = "")
    {
        if (user == null || user.Id <= 0 || string.IsNullOrWhiteSpace(password))
        {
            throw new ExplicitException("密码不能为空");
        }

        var maxFailedAttempts = ParseInt(await paramService.GetValueAsync(ConfigConst.UserPasswordMismatchLockCount), DefaultMaxFailedAttempts);
        var failedAttemptWindowMinutes = Math.Max(1, ParseInt(await paramService.GetValueAsync(ConfigConst.UserPasswordMismatchLockMinutes), DefaultFailedAttemptWindowMinutes));
        var lockEnabled = maxFailedAttempts > 0;
        var key = $"{CacheKeyConst.LockPasswordFailedPrefix}{user.Id}:{ipAddress}";
        if (lockEnabled)
        {
            var failedCount = await RedisHelper.StringGetAsync<long>(key);
            if (failedCount >= maxFailedAttempts)
            {
                throw new ExplicitException("密码错误次数过多，请稍后重试");
            }
        }

        if (user.Password != password)
        {
            if (lockEnabled)
            {
                var next = await RedisHelper.StringIncrementAsync(key);
                if (next == 1)
                {
                    await RedisHelper.KeyExpireAsync(key, TimeSpan.FromMinutes(failedAttemptWindowMinutes));
                }
                if (next >= maxFailedAttempts)
                {
                    throw new ExplicitException("密码错误次数过多，请稍后重试");
                }
            }
            throw new ExplicitException("密码错误");
        }

        await RedisHelper.KeyDeleteAsync(key);
        return true;
    }

    private static int ParseInt(string? value, int defaultValue) => int.TryParse(value, out var result) ? result : defaultValue;
}
