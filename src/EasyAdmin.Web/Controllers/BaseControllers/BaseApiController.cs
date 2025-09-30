using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Helper;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[UserAuth]
[ApiExceptionFilter]
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
    /// <summary>
    /// 当前用户角色
    /// </summary>
    protected UserRole UserRole => TenantContextHolder.UserRole;//UserInfo?.UserRole ?? UserRole.Unknown;

    #region ApiResult
    protected ApiResult Success()
    {
        return ApiResult.Ok();
    }
    protected ApiResult<T> Success<T>(T data)
    {
        return ApiResult.Ok<T>(data);
    }

    protected ApiResult Fail()
    {
        return ApiResult.Fail();
    }
    protected ApiResult Fail(string msg)
    {
        return ApiResult.Fail(msg);
    }
    protected ApiResult<T> Fail<T>()
    {
        return ApiResult.Fail<T>();
    }
    protected ApiResult<T> Fail<T>(string msg)
    {
        return ApiResult.Fail<T>(msg);
    }
    #endregion
}