using EasyAdmin.Web.Helper;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EasyAdmin.Web.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加Jwt服务
    /// </summary>
    public static void AddJwtService(this IServiceCollection services, JwtConfig jwtConfig)
    {
        JwtHelper.JwtConfig = jwtConfig;

        services.AddAuthentication(option =>
            {
                //认证middleware配置
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //Token颁发机构
                    ValidIssuer = JwtHelper.JwtConfig.Issuer,
                    //颁发给谁
                    ValidAudience = JwtHelper.JwtConfig.Audience,
                    //这里的key要进行加密
                    IssuerSigningKey = JwtHelper.JwtConfig.SymmetricSecurityKey,
                    //是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    //ClockSkew = TimeSpan.FromSeconds(30)// 缓冲时间，默认5分钟
                };
            });
    }
}