using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Web.Models;

/// <summary>
/// 在线会话记录
/// </summary>
public sealed record OnlineUserSessionRecord(
    long UserId,
    long TenantId,
    string IpAddress,
    DateTime LoginTime,
    string UserAgent,
    DateTime ExpiresAt);

/// <summary>
/// 在线用户汇总
/// </summary>
public sealed class OnlineUserSummary
{
    /// <summary>
    /// 用户编号
    /// </summary>
    public long UserId { get; init; }
    /// <summary>
    /// 租户编号
    /// </summary>
    public long TenantId { get; init; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// 用户昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;
    /// <summary>
    /// 登录 IP
    /// </summary>
    public string IpAddress { get; init; } = string.Empty;
    /// <summary>
    /// 最近登录时间
    /// </summary>
    public DateTime LoginTime { get; init; }
    /// <summary>
    /// 设备信息
    /// </summary>
    public string UserAgent { get; init; } = string.Empty;
    /// <summary>
    /// 在线会话数
    /// </summary>
    public int SessionCount { get; init; }
}

/// <summary>
/// 在线用户分页请求
/// </summary>
public sealed class OnlineUserPageRequest : PageRequestBase
{
    /// <summary>
    /// 用户名筛选条件
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// IP 筛选条件
    /// </summary>
    public string? IpAddress { get; set; }
}

/// <summary>
/// 强制下线请求
/// </summary>
public sealed class OnlineUserKickRequest
{
    /// <summary>
    /// 用户编号
    /// </summary>
    public long Id { get; set; }
}
