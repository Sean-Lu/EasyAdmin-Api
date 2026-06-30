namespace EasyAdmin.Web.Contracts;

public interface IAccountAccessService
{
    Task<bool> IsAllowedAsync(long tenantId, long userId);
    Task InvalidateTenantAsync(long tenantId);
    Task InvalidateUserAsync(long tenantId, long userId);
}
