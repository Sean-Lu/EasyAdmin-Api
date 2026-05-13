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
}