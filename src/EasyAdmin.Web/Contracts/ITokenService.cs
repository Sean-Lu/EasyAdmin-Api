using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Models;

namespace EasyAdmin.Web.Contracts;

/// <summary>
/// 令牌服务接口
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 生成访问令牌和刷新令牌
    /// </summary>
    /// <param name="user">用户模型</param>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="userAgent">用户代理</param>
    /// <returns>包含访问令牌和刷新令牌的元组</returns>
    Task<(string AccessToken, string RefreshToken)> GenerateTokens(JwtUserModel user, string ipAddress, string userAgent);
    /// <summary>
    /// 保存单 Token 会话
    /// </summary>
    Task StoreSingleTokenSessionAsync(JwtUserModel user, string token, string ipAddress, string userAgent);
    /// <summary>
    /// 获取单 Token 会话
    /// </summary>
    Task<SingleTokenSessionModel?> GetSingleTokenSessionAsync(long userId);
    /// <summary>
    /// 更新单 Token 会话令牌
    /// </summary>
    Task RenewSingleTokenSessionAsync(long userId, string token, DateTime expiresAt);
    /// <summary>
    /// 生成访问令牌
    /// </summary>
    /// <param name="user">用户模型</param>
    /// <returns>访问令牌</returns>
    string GenerateAccessToken(JwtUserModel user);
    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <returns>刷新令牌</returns>
    string GenerateRefreshToken();
    /// <summary>
    /// 刷新访问令牌和刷新令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="ipAddress">IP地址</param>
    /// <returns>包含是否成功、新访问令牌、新刷新令牌和消息的元组</returns>
    Task<(bool Success, string AccessToken, string NewRefreshToken, string Message)> RefreshTokenAsync(string refreshToken, string ipAddress);
    /// <summary>
    /// 撤销刷新令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    Task RevokeRefreshTokenAsync(string refreshToken);
    /// <summary>
    /// 撤销所有用户令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    Task RevokeAllUserTokensAsync(long userId);
    /// <summary>
    /// 获取在线会话记录
    /// </summary>
    /// <param name="tenantId">租户ID</param>
    Task<IReadOnlyList<OnlineUserSessionRecord>> GetOnlineSessionRecordsAsync(long tenantId);
    /// <summary>
    /// 强制撤销用户会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="reason">撤销原因</param>
    Task RevokeUserSessionsAsync(long userId, string reason);
    /// <summary>
    /// 检查令牌是否在黑名单中
    /// </summary>
    /// <param name="token">令牌</param>
    /// <returns>是否在黑名单中</returns>
    Task<bool> IsTokenBlacklistedAsync(string token);
    /// <summary>
    /// 添加令牌到黑名单
    /// </summary>
    /// <param name="token">令牌</param>
    /// <param name="reason">原因</param>
    Task AddTokenToBlacklistAsync(string token, string reason = "");
    /// <summary>
    /// 获取刷新令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <returns>刷新令牌模型</returns>
    Task<RefreshTokenModel?> GetRefreshTokenAsync(string refreshToken);
}
