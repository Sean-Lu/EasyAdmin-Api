namespace EasyAdmin.Infrastructure.Models;

public class JwtUserModel
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public long TenantId { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
}