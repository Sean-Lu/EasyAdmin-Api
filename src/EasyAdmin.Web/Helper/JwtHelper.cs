using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyAdmin.Web.Helper;

/// <summary>
/// JWT
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
    /// 生成token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string GenerateToken(JwtUserModel user)
    {
        var claims = new List<Claim>
        {
            new(nameof(JwtUserModel.TenantId),user.TenantId.ToString()),
            new(nameof(JwtUserModel.UserId),user.UserId.ToString()),
        };
        // token失效时间=过期时间+缓冲时间(ClockSkew)
        var jwtSecurityToken = new JwtSecurityToken(
            JwtConfig.Issuer,
            JwtConfig.Audience,
            claims,
            JwtConfig.NotBefore,
            JwtConfig.Expiration,// 过期时间
            JwtConfig.SigningCredentials
        );
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        token = "Bearer " + token;
        return token;
    }

    /// <summary>
    /// 获取token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string? GetToken(HttpRequest request)
    {
        // 先尝试从 Authorization 请求头获取
        if (request.Headers.TryGetValue(WebConst.RequestHeaderTokenKey, out var token))
        {
            var tokenStr = token.ToString();
            // 移除 Bearer 前缀（支持大小写不敏感）
            if (tokenStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return tokenStr.Substring("Bearer ".Length).Trim();
            }
            // 如果没有前缀，直接返回
            return tokenStr.Trim();
        }
        return null;
    }

    /// <summary>
    /// 获取Token过期时间。token失效时间=过期时间+缓冲时间(ClockSkew)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static DateTime? GetTokenExpiredTime(HttpRequest request)
    {
        return GetTokenExpiredTime(GetToken(request));
    }

    /// <summary>
    /// 获取Token过期时间。token失效时间=过期时间+缓冲时间(ClockSkew)
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
            // 手动解析 token 以获取 exp（因为 JwtSecurityTokenHandler 在某些情况下无法正确读取 exp）
            var parts = token.Split('.');
            if (parts.Length >= 2)
            {
                var payloadBase64 = parts[1];
                // 补全 base64 可能缺失的 =
                while (payloadBase64.Length % 4 != 0)
                    payloadBase64 += '=';

                var payloadBytes = Convert.FromBase64String(payloadBase64);
                var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

                // 解析 exp
                var jsonDoc = System.Text.Json.JsonDocument.Parse(payloadJson);
                if (jsonDoc.RootElement.TryGetProperty("exp", out var expElement) &&
                    expElement.TryGetInt64(out var expUnixTime))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(expUnixTime).UtcDateTime;
                }
            }
        }
        catch (Exception)
        {
            // 如果手动解析失败，尝试使用标准方法
        }

        // 备用：使用标准方法
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 验证token是否失效
    /// </summary>
    /// <param name="request"></param>
    /// <param name="exceptionHandler"></param>
    /// <returns>是否过期</returns>
    public static (bool, DateTime) IsTokenExpired(HttpRequest request, Action<Exception>? exceptionHandler = null)
    {
        return IsTokenExpired(GetToken(request), exceptionHandler);
    }

    /// <summary>
    /// 验证token是否失效
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

        //try
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    tokenHandler.ValidateToken(token, JwtConfig.TokenValidationParameters, out var validatedToken);
        //    var jwtToken = validatedToken as JwtSecurityToken;
        //    var validTo = jwtToken?.ValidTo.ToLocalTime();
        //    var effectiveExpiredTime = validTo?.Add(JwtConfig.TokenValidationParameters.ClockSkew) ?? DateTime.MinValue;
        //    //Console.WriteLine($"token过期时间：{validTo}，token失效时间：{effectiveExpiredTime}，当前时间：{DateTime.Now}");
        //    return (false, effectiveExpiredTime);
        //}
        //catch (Exception ex)
        //{
        //    // token 无效或已过期
        //    exceptionHandler?.Invoke(ex);
        //    return (true, DateTime.MinValue);
        //}

        var expiredTime = GetTokenExpiredTime(token);
        if (expiredTime == null)
        {
            return (true, DateTime.MinValue);
        }
        var clockSkew = JwtConfig.TokenValidationParameters.ClockSkew;
        var effectiveExpiredTime = expiredTime.Value.Add(clockSkew);
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
            var tenantId = request.Headers.TryGetValue(WebConst.RequestHeaderTenantIdKey, out var tenantIdStr)
                ? Convert.ToInt64(tenantIdStr) // 设置为请求头中携带的租户
                : SysConst.DefaultTenantId;// 设置为默认租户
            return new JwtUserModel
            {
                TenantId = tenantId
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