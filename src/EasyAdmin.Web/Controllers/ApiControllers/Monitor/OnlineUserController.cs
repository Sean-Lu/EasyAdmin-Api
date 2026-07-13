using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 在线用户
/// </summary>
public sealed class OnlineUserController(IOnlineUserService onlineUserService) : BaseApiController
{
    /// <summary>
    /// 分页查询在线用户
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<OnlineUserSummary>>> Page([FromQuery] OnlineUserPageRequest request)
    {
        return Success(await onlineUserService.PageAsync(request, TenantId));
    }

    /// <summary>
    /// 强制用户下线
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Kick([FromBody] OnlineUserKickRequest request)
    {
        await onlineUserService.KickAsync(request.Id, TenantId);
        return Success(true);
    }
}
