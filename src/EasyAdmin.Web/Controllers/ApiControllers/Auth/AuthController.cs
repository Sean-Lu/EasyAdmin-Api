using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Extensions;
using EasyAdmin.Web.Helper;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 授权
/// </summary>
public class AuthController(
    ILogger<AuthController> logger,
    IConfiguration configuration,
    IUserService userService,
    ILoginLogService loginLogService
    ) : BaseApiController
{
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ApiResult<LoginResponse>> Login(LoginRequest data)
    {
        var user = await userService.GetAsync(data.Username, data.Password);
        if (user == null || user.Id < 1)
        {
            return Fail<LoginResponse>("用户名或密码错误！");
        }

        if (user.State == CommonState.Disable)
        {
            return Fail<LoginResponse>("当前用户已被禁用，请联系管理员！");
        }

        var lastLoginTime = DateTime.Now;
        await userService.UpdateLastLoginTimeAsync(user.Id, lastLoginTime);// 更新用户最后登录时间
        await loginLogService.AddAsync(new LoginLogDto
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            LoginTime = lastLoginTime,
            IP = HttpContext.GetClientIp()
        });// 新增用户登录历史记录

        var token = JwtHelper.GetJwtToken(new JwtUserModel
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            UserRole = user.Role
        });
        return Success(new LoginResponse
        {
            AccessToken = token
        });
    }

    /// <summary>
    /// 验证Token是否过期
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public ApiResult<object> CheckToken()
    {
        var tokenExpiredTime = JwtHelper.GetTokenExpiredTime(this.Request);
        var isTokenExpired = tokenExpiredTime.HasValue && DateTime.UtcNow > tokenExpiredTime;// 如果当前时间已经超过过期时间，则Token过期
        return Success<object>(new
        {
            ValidTo = tokenExpiredTime,
            Expired = isTokenExpired
        });
    }

    /// <summary>
    /// 获取按钮权限
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ApiResult<object> Buttons()
    {
        return UserRole switch
        {
            UserRole.SuperAdministrator or UserRole.Administrator => Success<object>(new
            {
                useHooks = new Dictionary<string, bool> { { "add", true }, { "edit", true }, { "delete", true }, }
            }),
            UserRole.User => Success<object>(new
            {
                useHooks = new Dictionary<string, bool> { { "add", true }, { "edit", true }, }
            }),
            _ => Fail<object>()
        };
    }
}