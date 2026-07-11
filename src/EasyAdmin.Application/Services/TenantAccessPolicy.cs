using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Services;

public static class TenantAccessPolicy
{
    public static bool IsTenantValid(CommonState state, DateTime? startTime, DateTime? expireTime, DateTime now)
    {
        return state == CommonState.Enable
            && (!startTime.HasValue || now >= startTime.Value)
            && (!expireTime.HasValue || now < expireTime.Value);
    }

    public static TimeSpan GetCacheDuration(DateTime now, DateTime? nextBoundary)
    {
        if (!nextBoundary.HasValue)
        {
            return TimeSpan.FromSeconds(30);
        }

        var seconds = Math.Max(1, (nextBoundary.Value - now).TotalSeconds);
        return TimeSpan.FromSeconds(Math.Min(30, seconds));
    }
}
