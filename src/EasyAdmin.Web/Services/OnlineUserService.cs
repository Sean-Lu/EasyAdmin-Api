using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Models;

namespace EasyAdmin.Web.Services;

/// <summary>
/// 在线用户服务
/// </summary>
public sealed class OnlineUserService(IUserService userService, ITokenService tokenService) : IOnlineUserService
{
    public async Task<ApiResultPageData<OnlineUserSummary>> PageAsync(OnlineUserPageRequest request, long tenantId)
    {
        var records = OnlineUserSessionAggregator.Aggregate(
            await tokenService.GetOnlineSessionRecordsAsync(tenantId));
        var users = new List<OnlineUserSummary>();

        foreach (var item in records)
        {
            var user = await userService.GetByIdAsync(item.UserId);
            if (user is null || user.IsDelete || user.TenantId != tenantId) continue;
            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                var keyword = request.UserName.Trim();
                var matched = user.UserName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (user.NickName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false);
                if (!matched) continue;
            }
            if (!string.IsNullOrWhiteSpace(request.IpAddress) &&
                !item.IpAddress.Contains(request.IpAddress.Trim(), StringComparison.OrdinalIgnoreCase)) continue;

            item.UserName = user.UserName;
            item.NickName = user.NickName ?? string.Empty;
            users.Add(item);
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        return new ApiResultPageData<OnlineUserSummary>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Total = users.Count,
            List = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
    }

    public async Task KickAsync(long userId, long tenantId)
    {
        var user = await userService.GetByIdAsync(userId);
        if (user is null || user.IsDelete || user.TenantId != tenantId)
        {
            throw new ExplicitException("用户不存在或不属于当前租户");
        }

        var online = (await tokenService.GetOnlineSessionRecordsAsync(tenantId)).Any(item => item.UserId == userId);
        if (!online)
        {
            throw new ExplicitException("用户当前不在线");
        }

        await tokenService.RevokeUserSessionsAsync(userId, "管理员强制下线");
    }
}
