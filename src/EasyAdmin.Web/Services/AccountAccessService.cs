using EasyAdmin.Domain.Contracts;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Application.Services;
using EasyAdmin.Web.Contracts;
using Sean.Core.Redis;

namespace EasyAdmin.Web.Services;

public class AccountAccessService(
    ITenantRepository tenantRepository,
    IUserRepository userRepository) : IAccountAccessService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<bool> IsAllowedAsync(long tenantId, long userId)
    {
        if (tenantId < 1 || userId < 1 || !await GetTenantAllowedAsync(tenantId))
            return false;

        return await GetUserAllowedAsync(tenantId, userId);
    }

    public Task InvalidateTenantAsync(long tenantId) => RedisHelper.KeyDeleteAsync(GetTenantKey(tenantId));
    public Task InvalidateUserAsync(long tenantId, long userId) => RedisHelper.KeyDeleteAsync(GetUserKey(tenantId, userId));

    private async Task<bool> GetTenantAllowedAsync(long tenantId)
    {
        var key = GetTenantKey(tenantId);
        var cached = await RedisHelper.StringGetAsync<AccessStatusCache>(key);
        if (cached != null)
            return cached.Allowed;

        var tenant = await tenantRepository.GetAsync(entity => entity.Id == tenantId && !entity.IsDelete);
        var now = DateTime.UtcNow;
        var allowed = tenant != null && TenantAccessPolicy.IsTenantValid(tenant.State, tenant.StartTime, tenant.ExpireTime, now);
        var nextBoundary = tenant == null || tenant.StartTime > now
            ? tenant?.StartTime
            : tenant?.ExpireTime;
        var cacheDuration = TenantAccessPolicy.GetCacheDuration(now, nextBoundary);
        await RedisHelper.StringSetAsync(key, new AccessStatusCache { Allowed = allowed }, cacheDuration);
        return allowed;
    }

    private async Task<bool> GetUserAllowedAsync(long tenantId, long userId)
    {
        var key = GetUserKey(tenantId, userId);
        var cached = await RedisHelper.StringGetAsync<AccessStatusCache>(key);
        if (cached != null)
            return cached.Allowed;

        var user = await userRepository.GetAsync(entity => entity.Id == userId && entity.TenantId == tenantId && !entity.IsDelete);
        var allowed = user?.State == CommonState.Enable;
        await RedisHelper.StringSetAsync(key, new AccessStatusCache { Allowed = allowed }, CacheDuration);
        return allowed;
    }

    private static string GetTenantKey(long tenantId) => $"{CacheKeyConst.TenantAccessStatusPrefix}{tenantId}";
    private static string GetUserKey(long tenantId, long userId) => $"{CacheKeyConst.UserAccessStatusPrefix}{tenantId}:{userId}";

    private sealed class AccessStatusCache
    {
        public bool Allowed { get; set; }
    }
}
