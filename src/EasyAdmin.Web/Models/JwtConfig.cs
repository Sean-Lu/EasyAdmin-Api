using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EasyAdmin.Web.Models;

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
    /// 过期时间（min）
    /// </summary>
    public int Expired { get; set; }

    /// <summary>
    /// 生效时间
    /// </summary>
    public DateTime NotBefore => DateTime.Now;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime Expiration => DateTime.Now.AddMinutes(Expired);

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
}