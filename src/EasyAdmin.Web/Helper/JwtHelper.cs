using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Models;
using Microsoft.OpenApi.Models;
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
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    }
                },
                new string[] { }
            }
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT授权(数据将在请求头中进行传输) 在下方输入Bearer {token} 即可，注意两者之间有空格",
            Name = WebConst.RequestHeaderTokenKey,//jwt默认的参数名称
            In = ParameterLocation.Header,//jwt默认存放在请求头中
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
    }

    /// <summary>
    /// 生成token
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string GetJwtToken(JwtUserModel user)
    {
        var claims = new List<Claim>
        {
            new(nameof(JwtUserModel.TenantId),user.TenantId.ToString()),
            new(nameof(JwtUserModel.UserId),user.UserId.ToString()),
        };
        return GetJwtToken(claims);
    }

    /// <summary>
    /// 获取Token过期时间
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static DateTime? GetTokenExpiredTime(HttpRequest request)
    {
        var token = GetToken(request);
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

    /// <summary>
    /// 生成token
    /// </summary>
    /// <returns></returns>
    private static string GetJwtToken(List<Claim>? claims)
    {
        // token总过期时间=过期时间+缓冲时间
        var jwtSecurityToken = new JwtSecurityToken(
            JwtConfig.Issuer,
            JwtConfig.Audience,
            claims ?? new List<Claim>(),
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
    private static string? GetToken(HttpRequest request)
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
}