namespace EasyAdmin.Web.Models;

/// <summary>
/// 单 Token 在线会话
/// </summary>
public sealed class SingleTokenSessionModel
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 用户编号
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 租户编号
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 用户代理
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// 单 Token 会话转换器
/// </summary>
public static class SingleTokenSessionMapper
{
    /// <summary>
    /// 转换为在线会话记录
    /// </summary>
    public static OnlineUserSessionRecord ToOnlineSessionRecord(SingleTokenSessionModel session) =>
        new(session.UserId, session.TenantId, session.IpAddress, session.CreatedAt, session.UserAgent, session.ExpiresAt);
}
