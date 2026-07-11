using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Extensions;
using EasyAdmin.Web.Helper;
using EasyAdmin.Web.Middleware;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sean.Core.Redis;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 授权
/// </summary>
public class AuthController(
    ILogger<AuthController> logger,
    IConfiguration configuration,
    IUserService userService,
    ILoginLogService loginLogService,
    ITokenService tokenService,
    ICaptchaService captchaService,
    IParamService paramService,
    ITenantService tenantService,
    IAccountAccessService accountAccessService,
    AuthPasswordVerifier authPasswordVerifier
    ) : BaseApiController
{
    /// <summary>
    /// 获取登录配置
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResult<LoginConfigResponse>> LoginConfig()
    {
        return Success(new LoginConfigResponse
        {
            TenantEnabled = await paramService.GetBooleanValueAsync(ConfigConst.TenantEnable)
        });
    }

    /// <summary>
    /// 获取登录验证码
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResult<CaptchaResponse>> Captcha()
    {
        return Success(await captchaService.GenerateAsync());
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ApiResult<LoginResponse>> Login(LoginRequest data)
    {
        if (!await captchaService.ValidateAsync(data.CaptchaKey, data.CaptchaCode))
        {
            return Fail<LoginResponse>("验证码错误或已过期");
        }

        if (string.IsNullOrWhiteSpace(data.Account))
        {
            return Fail<LoginResponse>("账号不能为空！");
        }

        if (data.LoginType == LoginType.Password && string.IsNullOrWhiteSpace(data.Password))
        {
            return Fail<LoginResponse>("密码不能为空！");
        }

        var tenantEnabled = await paramService.GetBooleanValueAsync(ConfigConst.TenantEnable);
        var tenantSelection = TenantLoginPolicy.ResolveTenantCode(tenantEnabled, data.TenantCode);
        if (tenantSelection.TenantCode == null)
        {
            return Fail<LoginResponse>("租户、账号或密码错误！");
        }

        var tenant = tenantEnabled
            ? await tenantService.GetEnabledByCodeAsync(tenantSelection.TenantCode)
            : await tenantService.GetByIdAsync(SysConst.DefaultTenantId);
        if (tenant == null || tenant.Id < 1 || tenant.State == CommonState.Disable)
        {
            return Fail<LoginResponse>("租户、账号或密码错误！");
        }

        var password = data.Password ?? string.Empty;
        var user = await userService.GetByAccountAsync(data.Account, password, data.LoginType, tenant.Id);
        // 统一错误提示，避免账号枚举
        if (user == null || user.Id < 1)
        {
            return Fail<LoginResponse>("租户、账号或密码错误！");
        }

        if (user.State == CommonState.Disable)
        {
            return Fail<LoginResponse>("当前用户已被禁用，请联系管理员！");
        }

        var lastLoginTime = DateTime.Now;
        await userService.UpdateLastLoginTimeAsync(user.Id, user.TenantId, lastLoginTime);// 更新用户最后登录时间
        await loginLogService.AddAsync(new LoginLogDto
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            LoginType = data.LoginType,
            LoginTime = lastLoginTime,
            IP = HttpContext.GetClientIp()
        });// 新增用户登录历史记录

        if (JwtHelper.JwtConfig.TokenMode == TokenMode.Refresh)
        {
            // 双token模式
            var ipAddress = HttpContext.GetClientIp();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var (accessToken, refreshToken) = await tokenService.GenerateTokens(new JwtUserModel
            {
                TenantId = user.TenantId,
                UserId = user.Id
            }, ipAddress, userAgent);

            return Success(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = JwtHelper.JwtConfig.Expired * 60
            });
        }
        else
        {
            // 单token模式
            var token = JwtHelper.GenerateToken(new JwtUserModel
            {
                TenantId = user.TenantId,
                UserId = user.Id
            });

            if (JwtHelper.JwtConfig.UseSlidingExpiration)
            {
                var tokenKey = SlidingExpirationJwtMiddleware.GetTokenKey(user.Id);
                await RedisHelper.StringSetAsync(tokenKey, token.Replace("Bearer ", string.Empty), TimeSpan.FromMinutes(JwtHelper.JwtConfig.Expired));
            }

            return Success(new LoginResponse
            {
                AccessToken = token,
                ExpiresIn = JwtHelper.JwtConfig.Expired * 60
            });
        }
    }

    /// <summary>
    /// 刷新Token（双Token模式）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ApiResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return Fail<LoginResponse>("刷新令牌不能为空！");
        }

        if (JwtHelper.JwtConfig.TokenMode != TokenMode.Refresh)
        {
            return Fail<LoginResponse>("当前未启用双Token模式！");
        }

        var ipAddress = HttpContext.GetClientIp();
        var refreshTokenModel = await tokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (refreshTokenModel == null || !await accountAccessService.IsAllowedAsync(refreshTokenModel.TenantId, refreshTokenModel.UserId))
        {
            await tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            return Fail<LoginResponse>("当前租户或用户已被禁用");
        }
        var (success, accessToken, newRefreshToken, message) = await tokenService.RefreshTokenAsync(request.RefreshToken, ipAddress);

        if (!success)
        {
            return Fail<LoginResponse>(message);
        }

        return Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = JwtHelper.JwtConfig.Expired * 60
        });
    }

    /// <summary>
    /// 登出（使当前用户所有Token失效）
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> Logout()
    {
        var userInfo = JwtHelper.GetUserInfo(this.Request);
        if (userInfo.UserId <= 0)
        {
            return Fail<object>("用户未登录！");
        }

        if (JwtHelper.JwtConfig.TokenMode == TokenMode.Refresh)
        {
            await tokenService.RevokeAllUserTokensAsync(userInfo.UserId);
        }
        else
        {
            var token = JwtHelper.GetToken(this.Request);
            if (!string.IsNullOrEmpty(token))
            {
                await tokenService.AddTokenToBlacklistAsync(token, "用户主动登出");
            }
        }

        return Success<object>("登出成功！");
    }

    /// <summary>
    /// 验证当前密码
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> VerifyPassword(VerifyPasswordRequest request)
    {
        return Success(await authPasswordVerifier.VerifyAsync(UserId, request.Password, HttpContext.GetClientIp()));
    }

    /// <summary>
    /// 验证Token是否失效
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ApiResult<object>> CheckToken()
    {
        var token = JwtHelper.GetToken(this.Request);
        if (string.IsNullOrEmpty(token))
        {
            return Success<object>(new { Expired = true, Message = "Token为空" });
        }

        if (await tokenService.IsTokenBlacklistedAsync(token))
        {
            return Success<object>(new { Expired = true, Message = "Token已被禁用" });
        }

        var (isExpired, expiredTime) = JwtHelper.IsTokenExpired(token);
        
        if (JwtHelper.JwtConfig.TokenMode == TokenMode.Refresh)
        {
            var refreshToken = Request.Headers["X-Refresh-Token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshTokenModel = await tokenService.GetRefreshTokenAsync(refreshToken);
                if (refreshTokenModel != null && refreshTokenModel.ExpiresAt > DateTime.UtcNow)
                {
                    return Success<object>(new 
                    { 
                        Expired = false, 
                        AccessTokenExpired = isExpired,
                        Message = isExpired ? "AccessToken已过期，可使用RefreshToken刷新" : "Token有效"
                    });
                }
            }
        }

        return Success<object>(new 
        { 
            Expired = isExpired, 
            Message = isExpired ? "Token已过期" : "Token有效" 
        });
    }

    /// <summary>
    /// 获取按钮权限
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ApiResult<object> Buttons()
    {
        return Success<object>(new
        {
            useHooks = new Dictionary<string, bool> { { "add", true }, { "edit", true }, { "delete", true }, }
        });
    }
}
