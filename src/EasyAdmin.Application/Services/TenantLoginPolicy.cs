using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Application.Services;

public static class TenantLoginPolicy
{
    public const int MaxTenantCodeLength = 50;

    public static TenantLoginSelection ResolveTenantCode(bool tenantEnabled, string? tenantCode)
    {
        if (!tenantEnabled)
        {
            return new TenantLoginSelection(false, SysConst.DefaultTenantCode);
        }

        var normalizedCode = tenantCode?.Trim();
        if (string.IsNullOrEmpty(normalizedCode) || normalizedCode.Length > MaxTenantCodeLength)
        {
            return new TenantLoginSelection(true, null);
        }

        return new TenantLoginSelection(true, normalizedCode);
    }
}

public readonly record struct TenantLoginSelection(bool TenantCodeRequired, string? TenantCode);
