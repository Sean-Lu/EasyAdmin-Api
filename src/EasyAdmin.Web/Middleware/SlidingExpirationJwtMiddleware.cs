using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Helper;
using EasyAdmin.Web.Models;
using Sean.Core.Redis;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Web.Middleware;

/// <summary>
/// JWT滑动过期中间件
/// </summary>
public class SlidingExpirationJwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtConfig _jwtConfig;
    private readonly ITokenService _tokenService;

    public SlidingExpirationJwtMiddleware(RequestDelegate next, JwtConfig jwtConfig, ITokenService tokenService)
    {
        _next = next;
        _jwtConfig = jwtConfig;
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = JwtHelper.GetToken(context.Request);
        if (!string.IsNullOrEmpty(token))
        {
            if (await _tokenService.IsTokenBlacklistedAsync(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    code = 401,
                    success = false,
                    msg = "Token已被禁用，请重新登录"
                });
                return;
            }
        }

        if (_jwtConfig.TokenMode == TokenMode.Refresh)
        {
            await _next(context);
            return;
        }

        if (!_jwtConfig.UseSlidingExpiration)
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrEmpty(token))
        {
            await _next(context);
            return;
        }

        try
        {
            var (isExpired, expiredTime) = JwtHelper.IsTokenExpired(token);
            if (isExpired)
            {
                await _next(context);
                return;
            }

            var remainingTime = expiredTime - DateTime.UtcNow;
            if (remainingTime.TotalMinutes < _jwtConfig.SlidingExpirationThreshold)
            {
                var user = context.User;
                if (user.Identity != null && user.Identity.IsAuthenticated)
                {
                    var tenantIdClaim = user.FindFirst(nameof(JwtUserModel.TenantId));
                    var userIdClaim = user.FindFirst(nameof(JwtUserModel.UserId));
                    if (tenantIdClaim != null && userIdClaim != null &&
                        long.TryParse(tenantIdClaim.Value, out var tenantId) &&
                        long.TryParse(userIdClaim.Value, out var userId))
                    {
                        var newToken = JwtHelper.GenerateToken(new JwtUserModel
                        {
                            TenantId = tenantId,
                            UserId = userId
                        });

                        await _tokenService.RenewSingleTokenSessionAsync(
                            userId,
                            newToken.Replace("Bearer ", string.Empty),
                            JwtHelper.GetTokenExpiredTime(newToken) ?? DateTime.UtcNow.AddMinutes(_jwtConfig.Expired));

                        context.Response.Headers["X-New-Token"] = newToken;
                    }
                }
            }
        }
        catch
        {
        }

        await _next(context);
    }

    public static string GetTokenKey(long userId)
    {
        return $"{CacheKeyConst.TokenPrefix}{userId}";
    }
}