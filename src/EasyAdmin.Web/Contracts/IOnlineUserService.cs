using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;

namespace EasyAdmin.Web.Contracts;

/// <summary>
/// 在线用户服务
/// </summary>
public interface IOnlineUserService
{
    /// <summary>
    /// 分页查询当前租户内的在线用户
    /// </summary>
    /// <param name="request">分页和筛选条件</param>
    /// <param name="tenantId">当前租户编号</param>
    /// <returns>在线用户分页结果</returns>
    Task<ApiResultPageData<OnlineUserSummary>> PageAsync(OnlineUserPageRequest request, long tenantId);
    /// <summary>
    /// 强制注销用户的全部在线会话
    /// </summary>
    /// <param name="userId">用户编号</param>
    /// <param name="tenantId">当前租户编号</param>
    Task KickAsync(long userId, long tenantId);
}
