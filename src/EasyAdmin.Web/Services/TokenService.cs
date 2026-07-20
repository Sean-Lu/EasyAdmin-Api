using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Helper;
using EasyAdmin.Web.Middleware;
using EasyAdmin.Web.Models;
using Sean.Core.Redis;

namespace EasyAdmin.Web.Services;

public class TokenService(JwtConfig jwtConfig) : ITokenService
{
    public async Task<(string AccessToken, string RefreshToken)> GenerateTokens(JwtUserModel user, string ipAddress, string userAgent)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        if (jwtConfig.TokenMode == TokenMode.Refresh)
        {
            await StoreRefreshToken(refreshToken, user, ipAddress, userAgent);
        }

        return (accessToken, refreshToken);
    }

    public string GenerateAccessToken(JwtUserModel user)
    {
        var claims = new List<Claim>
        {
            new(nameof(JwtUserModel.TenantId), user.TenantId.ToString()),
            new(nameof(JwtUserModel.UserId), user.UserId.ToString()),
        };

        var jwtSecurityToken = new JwtSecurityToken(
            jwtConfig.Issuer,
            jwtConfig.Audience,
            claims,
            jwtConfig.NotBefore,
            jwtConfig.Expiration,
            jwtConfig.SigningCredentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return "Bearer " + token;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task StoreSingleTokenSessionAsync(JwtUserModel user, string token, string ipAddress, string userAgent)
    {
        var session = new SingleTokenSessionModel
        {
            Token = token,
            UserId = user.UserId,
            TenantId = user.TenantId,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = userAgent ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = JwtHelper.GetTokenExpiredTime(token) ?? DateTime.UtcNow.AddMinutes(jwtConfig.Expired)
        };

        await RedisHelper.StringSetAsync(
            SlidingExpirationJwtMiddleware.GetTokenKey(user.UserId),
            session,
            session.ExpiresAt - DateTime.UtcNow);
    }

    public async Task<SingleTokenSessionModel?> GetSingleTokenSessionAsync(long userId) =>
        await RedisHelper.StringGetAsync<SingleTokenSessionModel>(SlidingExpirationJwtMiddleware.GetTokenKey(userId));

    public async Task RenewSingleTokenSessionAsync(long userId, string token, DateTime expiresAt)
    {
        var session = await GetSingleTokenSessionAsync(userId);
        if (session is null) return;

        session.Token = token;
        session.ExpiresAt = expiresAt;
        await RedisHelper.StringSetAsync(
            SlidingExpirationJwtMiddleware.GetTokenKey(userId),
            session,
            expiresAt - DateTime.UtcNow);
    }

    private async Task StoreRefreshToken(string refreshToken, JwtUserModel user, string ipAddress, string userAgent)
    {
        var refreshTokenModel = new RefreshTokenModel
        {
            UserId = user.UserId,
            TenantId = user.TenantId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtConfig.RefreshTokenExpired),
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        var refreshTokenKey = $"{CacheKeyConst.RefreshTokenPrefix}{refreshToken}";
        var userTokensKey = $"{CacheKeyConst.UserRefreshTokensPrefix}{user.UserId}";

        await RedisHelper.StringSetAsync(refreshTokenKey, refreshTokenModel, TimeSpan.FromMinutes(jwtConfig.RefreshTokenExpired));
        await RedisHelper.ListRightPushAsync(userTokensKey, refreshToken);
        await RedisHelper.KeyExpireAsync(userTokensKey, TimeSpan.FromMinutes(jwtConfig.RefreshTokenExpired));
    }

    public async Task<(bool Success, string AccessToken, string NewRefreshToken, string Message)> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var refreshTokenModel = await GetRefreshTokenAsync(refreshToken);
        if (refreshTokenModel == null)
        {
            return (false, "", "", "无效的刷新令牌");
        }

        if (refreshTokenModel.ExpiresAt < DateTime.UtcNow)
        {
            await RevokeRefreshTokenAsync(refreshToken);
            return (false, "", "", "刷新令牌已过期");
        }

        var user = new JwtUserModel
        {
            UserId = refreshTokenModel.UserId,
            TenantId = refreshTokenModel.TenantId
        };

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        await RevokeRefreshTokenAsync(refreshToken);

        var newRefreshTokenModel = new RefreshTokenModel
        {
            UserId = user.UserId,
            TenantId = user.TenantId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtConfig.RefreshTokenExpired),
            IpAddress = ipAddress,
            UserAgent = refreshTokenModel.UserAgent
        };

        var refreshTokenKey = $"{CacheKeyConst.RefreshTokenPrefix}{newRefreshToken}";
        var userTokensKey = $"{CacheKeyConst.UserRefreshTokensPrefix}{user.UserId}";

        await RedisHelper.StringSetAsync(refreshTokenKey, newRefreshTokenModel, TimeSpan.FromMinutes(jwtConfig.RefreshTokenExpired));
        await RedisHelper.ListRightPushAsync(userTokensKey, newRefreshToken);
        await RedisHelper.KeyExpireAsync(userTokensKey, TimeSpan.FromMinutes(jwtConfig.RefreshTokenExpired));

        return (true, newAccessToken, newRefreshToken, "");
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var refreshTokenModel = await GetRefreshTokenAsync(refreshToken);
        if (refreshTokenModel == null) return;

        var refreshTokenKey = $"{CacheKeyConst.RefreshTokenPrefix}{refreshToken}";
        var userTokensKey = $"{CacheKeyConst.UserRefreshTokensPrefix}{refreshTokenModel.UserId}";

        await RedisHelper.KeyDeleteAsync(refreshTokenKey);
        await RedisHelper.ListRemoveAsync(userTokensKey, refreshToken);
    }

    public async Task RevokeAllUserTokensAsync(long userId)
    {
        var userTokensKey = $"{CacheKeyConst.UserRefreshTokensPrefix}{userId}";
        var refreshTokens = await RedisHelper.ListRangeAsync<string>(userTokensKey);

        foreach (var refreshToken in refreshTokens)
        {
            var refreshTokenKey = $"{CacheKeyConst.RefreshTokenPrefix}{refreshToken}";
            await RedisHelper.KeyDeleteAsync(refreshTokenKey);
        }

        await RedisHelper.KeyDeleteAsync(userTokensKey);
    }

    /// <summary>
    /// 读取指定租户的在线会话记录
    /// </summary>
    /// <param name="tenantId">租户编号</param>
    /// <returns>未过期的在线会话记录</returns>
    public async Task<IReadOnlyList<OnlineUserSessionRecord>> GetOnlineSessionRecordsAsync(long tenantId)
    {
        var now = DateTime.UtcNow;
        return jwtConfig.TokenMode == TokenMode.Refresh
            ? await GetRefreshSessionRecordsAsync(tenantId, now)
            : await GetSingleSessionRecordsAsync(tenantId, now);
    }

    /// <summary>
    /// 注销用户的全部在线会话
    /// </summary>
    /// <param name="userId">用户编号</param>
    /// <param name="reason">注销原因</param>
    public async Task RevokeUserSessionsAsync(long userId, string reason)
    {
        if (jwtConfig.TokenMode == TokenMode.Refresh)
        {
            await RevokeAllUserTokensAsync(userId);
            return;
        }

        var tokenKey = SlidingExpirationJwtMiddleware.GetTokenKey(userId);
        var session = await GetSingleTokenSessionAsync(userId);
        if (!string.IsNullOrWhiteSpace(session?.Token))
        {
            await AddTokenToBlacklistAsync(session.Token, reason);
        }

        await RedisHelper.KeyDeleteAsync(tokenKey);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var tokenHash = GetTokenHash(token);
        var blacklistKey = $"{CacheKeyConst.TokenBlacklistPrefix}{tokenHash}";
        return await RedisHelper.KeyExistsAsync(blacklistKey);
    }

    public async Task AddTokenToBlacklistAsync(string token, string reason = "")
    {
        if (string.IsNullOrEmpty(token))
            return;

        var tokenExpiry = JwtHelper.GetTokenExpiredTime(token);
        if (tokenExpiry == null)
            return;

        var tokenHash = GetTokenHash(token);
        var blacklistKey = $"{CacheKeyConst.TokenBlacklistPrefix}{tokenHash}";
        var expiresIn = tokenExpiry.Value - DateTime.UtcNow;

        if (expiresIn > TimeSpan.Zero)
        {
            var blacklistItem = new TokenBlacklistItem
            {
                Token = tokenHash,
                ExpiresAt = tokenExpiry.Value,
                Reason = reason
            };
            await RedisHelper.StringSetAsync(blacklistKey, blacklistItem, expiresIn);
        }
    }

    public async Task<RefreshTokenModel?> GetRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var refreshTokenKey = $"{CacheKeyConst.RefreshTokenPrefix}{refreshToken}";
        return await RedisHelper.StringGetAsync<RefreshTokenModel>(refreshTokenKey);
    }

    private string GetTokenHash(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private static async Task<IReadOnlyList<OnlineUserSessionRecord>> GetRefreshSessionRecordsAsync(long tenantId, DateTime now)
    {
        var records = new List<OnlineUserSessionRecord>();
        foreach (var key in await ScanKeysAsync(CacheKeyConst.RefreshTokenPrefix + "*"))
        {
            try
            {
                var session = await RedisHelper.StringGetAsync<RefreshTokenModel>(key);
                if (session is null || session.TenantId != tenantId || session.ExpiresAt <= now)
                {
                    continue;
                }

                records.Add(new OnlineUserSessionRecord(
                    session.UserId,
                    session.TenantId,
                    session.IpAddress ?? string.Empty,
                    session.CreatedAt,
                    session.UserAgent ?? string.Empty,
                    session.ExpiresAt));
            }
            catch
            {
                // 跳过单条无效会话
            }
        }

        return records;
    }

    private static async Task<IReadOnlyList<OnlineUserSessionRecord>> GetSingleSessionRecordsAsync(long tenantId, DateTime now)
    {
        var records = new List<OnlineUserSessionRecord>();
        foreach (var key in await ScanKeysAsync(CacheKeyConst.TokenPrefix + "*"))
        {
            try
            {
                var session = await RedisHelper.StringGetAsync<SingleTokenSessionModel>(key);
                if (session is not null && session.TenantId == tenantId && session.ExpiresAt > now)
                {
                    records.Add(SingleTokenSessionMapper.ToOnlineSessionRecord(session));
                }
            }
            catch
            {
                // 跳过单条无效会话
            }
        }

        return records;
    }

    private static async Task<List<string>> ScanKeysAsync(string pattern)
    {
        var keys = new List<string>();
        long cursor = 0;
        do
        {
            var result = await RedisHelper.ExecuteAsync(database => database.ExecuteAsync("SCAN", cursor, "MATCH", pattern, "COUNT", 200), 0);
            var parts = (StackExchange.Redis.RedisResult[])result;
            cursor = (long)parts[0];
            keys.AddRange(((StackExchange.Redis.RedisResult[])parts[1]).Select(item => (string)item));
        } while (cursor != 0);

        return keys;
    }
}
