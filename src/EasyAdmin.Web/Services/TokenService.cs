using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Helper;
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
}