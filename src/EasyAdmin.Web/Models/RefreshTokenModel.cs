namespace EasyAdmin.Web.Models;

/// <summary>
/// 刷新令牌模型
/// </summary>
public class RefreshTokenModel
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 租户ID
    /// </summary>
    public long TenantId { get; set; }
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string Token { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// IP地址
    /// </summary>
    public string IpAddress { get; set; }
    /// <summary>
    /// 用户代理
    /// </summary>
    public string UserAgent { get; set; }
}

/// <summary>
/// 刷新令牌请求模型
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; set; }
}

/// <summary>
/// 令牌黑名单项模型
/// </summary>
public class TokenBlacklistItem
{
    /// <summary>
    /// 令牌
    /// </summary>
    public string Token { get; set; }
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>
    /// 添加时间
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; set; }
}