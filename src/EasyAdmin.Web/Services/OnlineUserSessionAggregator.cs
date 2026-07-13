using EasyAdmin.Web.Models;

namespace EasyAdmin.Web.Services;

/// <summary>
/// 在线会话聚合器
/// </summary>
public static class OnlineUserSessionAggregator
{
    /// <summary>
    /// 过滤已过期会话并按租户和用户聚合
    /// </summary>
    /// <param name="records">在线会话记录</param>
    /// <param name="now">用于判断过期的当前时间</param>
    /// <returns>按最近登录时间倒序排列的在线用户</returns>
    public static List<OnlineUserSummary> Aggregate(IEnumerable<OnlineUserSessionRecord> records, DateTime? now = null)
    {
        var currentTime = now ?? DateTime.UtcNow;

        return records
            .Where(record => record.ExpiresAt > currentTime)
            .GroupBy(record => new { record.TenantId, record.UserId })
            .Select(group =>
            {
                var latest = group.OrderByDescending(record => record.LoginTime).First();
                return new OnlineUserSummary
                {
                    UserId = group.Key.UserId,
                    TenantId = group.Key.TenantId,
                    IpAddress = latest.IpAddress,
                    LoginTime = latest.LoginTime,
                    UserAgent = latest.UserAgent,
                    SessionCount = group.Count()
                };
            })
            .OrderByDescending(item => item.LoginTime)
            .ToList();
    }
}
