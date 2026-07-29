using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Helper;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// API控制器基类
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
[UserAuth]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// 当前用户信息
    /// </summary>
    protected JwtUserModel? UserInfo => TenantContextHolder.UserInfo;//JwtHelper.GetUserInfo(this.Request);
    /// <summary>
    /// 当前租户ID
    /// </summary>
    protected long TenantId => TenantContextHolder.TenantId;//UserInfo?.TenantId ?? 0;
    /// <summary>
    /// 当前用户ID
    /// </summary>
    protected long UserId => TenantContextHolder.UserId;//UserInfo?.UserId ?? 0;

    #region ApiResult
    /// <summary>
    /// 返回成功结果
    /// </summary>
    protected ApiResult Success()
    {
        return ApiResult.Ok();
    }
    /// <summary>
    /// 返回带数据的成功结果
    /// </summary>
    protected ApiResult<T> Success<T>(T data)
    {
        return ApiResult.Ok<T>(data);
    }

    /// <summary>
    /// 返回失败结果
    /// </summary>
    protected ApiResult Fail()
    {
        return ApiResult.Fail();
    }
    /// <summary>
    /// 返回带消息的失败结果
    /// </summary>
    protected ApiResult Fail(string msg)
    {
        return ApiResult.Fail(msg);
    }
    /// <summary>
    /// 返回泛型失败结果
    /// </summary>
    protected ApiResult<T> Fail<T>()
    {
        return ApiResult.Fail<T>();
    }
    /// <summary>
    /// 返回带消息的泛型失败结果
    /// </summary>
    protected ApiResult<T> Fail<T>(string msg)
    {
        return ApiResult.Fail<T>(msg);
    }
    #endregion
}