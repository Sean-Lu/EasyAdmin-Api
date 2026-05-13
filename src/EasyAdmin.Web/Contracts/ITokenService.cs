using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Models;

namespace EasyAdmin.Web.Contracts;

public interface ITokenService
{
    Task<(string AccessToken, string RefreshToken)> GenerateTokens(JwtUserModel user, string ipAddress, string userAgent);
    string GenerateAccessToken(JwtUserModel user);
    string GenerateRefreshToken();
    Task<(bool Success, string AccessToken, string NewRefreshToken, string Message)> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllUserTokensAsync(long userId);
    Task<bool> IsTokenBlacklistedAsync(string token);
    Task AddTokenToBlacklistAsync(string token, string reason = "");
    Task<RefreshTokenModel?> GetRefreshTokenAsync(string refreshToken);
}