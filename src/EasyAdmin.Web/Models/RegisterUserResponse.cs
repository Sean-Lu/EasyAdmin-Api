namespace EasyAdmin.Web.Models;

/// <summary>
/// 用户注册结果
/// </summary>
public class RegisterUserResponse
{
    /// <summary>
    /// 是否需要审核
    /// </summary>
    public bool RequiresApproval { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// 手机号
    /// </summary>
    public string? PhoneNumber { get; set; }
    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }
    /// <summary>
    /// 租户编码
    /// </summary>
    public string? TenantCode { get; set; }
}
