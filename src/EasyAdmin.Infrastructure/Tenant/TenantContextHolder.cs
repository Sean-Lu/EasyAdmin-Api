using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;

namespace EasyAdmin.Infrastructure.Tenant;

/// <summary>
/// 多租户上下文
/// </summary>
public static class TenantContextHolder
{
    private static readonly AsyncLocal<JwtUserModel?> JWT_USER_INFO = new();

    /// <summary>
    /// 当前用户信息
    /// </summary>
    public static JwtUserModel? UserInfo
    {
        get => JWT_USER_INFO.Value;
        set => JWT_USER_INFO.Value = value;
    }

    /// <summary>
    /// 租户ID
    /// </summary>
    public static long TenantId => (UserInfo?.TenantId).GetValueOrDefault();
    /// <summary>
    /// 用户ID
    /// </summary>
    public static long UserId => (UserInfo?.UserId).GetValueOrDefault();
    /// <summary>
    /// 用户角色
    /// </summary>
    public static UserRole UserRole => (UserInfo?.UserRole).GetValueOrDefault();

    /// <summary>
    /// 清理上下文
    /// </summary>
    public static void Clear()
    {
        JWT_USER_INFO.Value = default;
    }
}