namespace EasyAdmin.Infrastructure.Const;

/// <summary>
/// 配置常量
/// </summary>
public class ConfigConst
{
    /// <summary>
    /// 是否开启多租户功能
    /// </summary>
    public const string TenantEnable = "tenant.enable";
    /// <summary>
    /// 租管账号初始密码
    /// </summary>
    public const string TenantAdminInitPassword = "tenant.adminInitPassword";

    /// <summary>
    /// 是否开启注册功能
    /// </summary>
    public const string UserEnableRegister = "user.enableRegister";
    /// <summary>
    /// 账号初始密码
    /// </summary>
    public const string UserInitPassword = "user.initPassword";
    /// <summary>
    /// 账号密码错误锁定次数
    /// </summary>
    public const string UserPasswordMismatchLockCount = "user.passwordMismatchLockCount";
}