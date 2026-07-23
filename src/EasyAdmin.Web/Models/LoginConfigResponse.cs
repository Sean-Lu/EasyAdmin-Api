namespace EasyAdmin.Web.Models;

/// <summary>
/// 登录配置
/// </summary>
public class LoginConfigResponse
{
    /// <summary>
    /// 是否启用多租户
    /// </summary>
    public bool TenantEnabled { get; set; }
    /// <summary>
    /// 是否开启注册
    /// </summary>
    public bool RegisterEnabled { get; set; }
}
