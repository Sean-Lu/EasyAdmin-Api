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
                options.TokenValidationParameters = JwtHelper.JwtConfig.TokenValidationParameters;
            });
    }
}