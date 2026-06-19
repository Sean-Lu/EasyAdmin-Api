using System.Text;
using EasyAdmin.Infrastructure.Enums;
using Microsoft.IdentityModel.Tokens;

namespace EasyAdmin.Web.Models;

/// <summary>
/// JWT配置模型
/// </summary>
public class JwtConfig
{
    /// <summary>
    /// 密钥
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// 发布者
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// 接受者
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    /// Token模式：Single(单Token)/Refresh(双Token)
    /// </summary>
    public TokenMode TokenMode { get; set; } = TokenMode.Single;

    /// <summary>
    /// 是否启用滑动过期（仅单Token模式有效）
    /// </summary>
    public bool UseSlidingExpiration { get; set; }

    /// <summary>
    /// 滑动过期阈值（分钟），token剩余时间小于此值时自动刷新
    /// </summary>
    public int SlidingExpirationThreshold { get; set; } = 30;

    /// <summary>
    /// AccessToken过期时间（分钟）
    /// </summary>
    public int Expired { get; set; } = 30;

    /// <summary>
    /// RefreshToken过期时间（分钟），仅双Token模式有效，默认7天(10080分钟)
    /// </summary>
    public int RefreshTokenExpired { get; set; } = 10080;

    /// <summary>
    /// 生效时间
    /// </summary>
    public DateTime NotBefore => DateTime.UtcNow;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime Expiration => DateTime.UtcNow.AddMinutes(Expired);

    /// <summary>
    /// 密钥Bytes
    /// </summary>
    private SecurityKey SigningKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

    /// <summary>
    /// 加密后的密钥，使用HmacSha256加密
    /// </summary>
    public SigningCredentials SigningCredentials => new(SigningKey, SecurityAlgorithms.HmacSha256);

    /// <summary>
    /// 认证用的密钥
    /// </summary>
    public SymmetricSecurityKey SymmetricSecurityKey => new(Encoding.UTF8.GetBytes(SecretKey));

    /// <summary>
    /// Token验证参数
    /// </summary>
    public TokenValidationParameters TokenValidationParameters => new()
    {
        // Token颁发机构
        ValidIssuer = Issuer,
        // 颁发给谁
        ValidAudience = Audience,
        // 这里的key要进行加密
        IssuerSigningKey = SymmetricSecurityKey,
        // 是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
        ValidateLifetime = true,
        // 是否验证Token的发布者
        ValidateIssuer = true,
        // 是否验证Token的接受者
        ValidateAudience = true,
        // 是否验证Token的签名密钥
        ValidateIssuerSigningKey = true,
        // 是否要求Token过期时间
        RequireExpirationTime = true
        // 时钟偏差/缓冲时间，默认值：5分钟（即使 token 已经过期，但只要在过期时间 + 缓冲时间内，认证仍然会通过）
        // 安全提示‌：ClockSkew 不宜设得过大，否则会延长令牌的有效期，增加安全风险。建议范围为 ‌0 到 5 分钟‌‌‌
        //ClockSkew = TimeSpan.FromMinutes(2)
    };
}