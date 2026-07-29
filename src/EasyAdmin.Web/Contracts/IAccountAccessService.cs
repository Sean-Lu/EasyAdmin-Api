namespace EasyAdmin.Web.Contracts;

/// <summary>
/// 账户访问服务接口
/// </summary>
public interface IAccountAccessService
{
    /// <summary>
    /// 检查账户是否允许访问
    /// </summary>
    Task<bool> IsAllowedAsync(long tenantId, long userId);

    /// <summary>
    /// 清除租户访问缓存
    /// </summary>
    Task InvalidateTenantAsync(long tenantId);

    /// <summary>
    /// 清除用户访问缓存
    /// </summary>
    Task InvalidateUserAsync(long tenantId, long userId);
}
