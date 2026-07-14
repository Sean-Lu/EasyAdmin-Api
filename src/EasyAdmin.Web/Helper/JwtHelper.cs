using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyAdmin.Web.Helper;

/// <summary>
/// JWT帮助类
/// </summary>
public static class JwtHelper
{
    public static JwtConfig JwtConfig { get; set; }

    /// <summary>
    /// Swagger添加Jwt功能
    /// </summary>
    /// <param name="options"></param>
    public static void SwaggerAddJwt(SwaggerGenOptions options)
    {
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new()
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = WebConst.RequestHeaderTokenKey,
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
    }

    /// <summary>
    /// 生成Token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string GenerateToken(JwtUserModel user)
    {
        var claims = new List<Claim>
        {
            new(nameof(JwtUserModel.TenantId), user.TenantId.ToString()),
            new(nameof(JwtUserModel.UserId), user.UserId.ToString()),
        };
        var jwtSecurityToken = new JwtSecurityToken(
            JwtConfig.Issuer,
            JwtConfig.Audience,
            claims,
            JwtConfig.NotBefore,
            JwtConfig.Expiration,
            JwtConfig.SigningCredentials
        );
        return "Bearer " + new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    /// <summary>
    /// 获取Token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string? GetToken(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(WebConst.RequestHeaderTokenKey, out var token))
        {
            return null;
        }

        var tokenStr = token.ToString();
        return tokenStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? tokenStr.Substring("Bearer ".Length).Trim()
            : tokenStr.Trim();
    }

    /// <summary>
    /// 获取Token过期时间
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static DateTime? GetTokenExpiredTime(HttpRequest request)
    {
        return GetTokenExpiredTime(GetToken(request));
    }

    /// <summary>
    /// 获取Token过期时间
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static DateTime? GetTokenExpiredTime(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 验证Token是否过期
    /// </summary>
    /// <param name="request"></param>
    /// <param name="exceptionHandler"></param>
    /// <returns>是否过期</returns>
    public static (bool, DateTime) IsTokenExpired(HttpRequest request, Action<Exception>? exceptionHandler = null)
    {
        return IsTokenExpired(GetToken(request), exceptionHandler);
    }

    /// <summary>
    /// 验证Token是否过期
    /// </summary>
    /// <param name="token"></param>
    /// <param name="exceptionHandler"></param>
    /// <returns>是否过期</returns>
    public static (bool, DateTime) IsTokenExpired(string? token, Action<Exception>? exceptionHandler = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            return (true, DateTime.MinValue);
        }

        var expiredTime = GetTokenExpiredTime(token);
        if (expiredTime == null)
        {
            return (true, DateTime.MinValue);
        }
        var effectiveExpiredTime = expiredTime.Value.Add(JwtConfig.TokenValidationParameters.ClockSkew);
        var isExpired = DateTime.UtcNow > effectiveExpiredTime;
        return (isExpired, effectiveExpiredTime);
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static JwtUserModel GetUserInfo(HttpRequest request)
    {
        var user = request.HttpContext.User;
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            //var tenantId = request.Headers.TryGetValue(WebConst.RequestHeaderTenantIdKey, out var tenantIdStr)
            //    ? Convert.ToInt64(tenantIdStr) // 设置为请求头中携带的租户
            //    : SysConst.DefaultTenantId;// 设置为默认租户
            return new JwtUserModel
            {
                TenantId = SysConst.DefaultTenantId
            };
        }

        var claims = user.Claims;
        return new JwtUserModel
        {
            TenantId = Convert.ToInt64(claims.First(t => t.Type == nameof(JwtUserModel.TenantId)).Value),
            UserId = Convert.ToInt64(claims.First(t => t.Type == nameof(JwtUserModel.UserId)).Value),
        };
    }
}