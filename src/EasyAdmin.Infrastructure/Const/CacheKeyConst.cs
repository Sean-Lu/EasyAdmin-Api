namespace EasyAdmin.Infrastructure.Const;

/// <summary>
/// 缓存 Key 常量
/// </summary>
public class CacheKeyConst
{
    public const string TopPrefix = "EasyAdmin:";

    public const string TokenPrefix = $"{TopPrefix}Token:";
    public const string RefreshTokenPrefix = $"{TopPrefix}RefreshToken:";
    public const string TokenBlacklistPrefix = $"{TopPrefix}TokenBlacklist:";
    public const string UserRefreshTokensPrefix = $"{TopPrefix}UserRefreshTokens:";

    public const string ApiRepeatRequestLimitPrefix = $"{TopPrefix}RepeatRequestLimit:";
    public const string LockPasswordFailedPrefix = $"{TopPrefix}LockPasswordFailed:";

    public const string CaptchaPrefix = $"{TopPrefix}Captcha:";
    public const string CaptchaLockPrefix = $"{TopPrefix}CaptchaLock:";

    public const string TenantAccessStatusPrefix = $"{TopPrefix}TenantAccessStatus:";
    public const string UserAccessStatusPrefix = $"{TopPrefix}UserAccessStatus:";
}
