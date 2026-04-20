namespace EasyAdmin.Infrastructure.Const;

/// <summary>
/// 系统常量
/// </summary>
public class SysConst
{
    /// <summary>
    /// 系统默认内置租户ID
    /// </summary>
    public const long DefaultTenantId = 1;

    /// <summary>
    /// 超级管理员用户ID
    /// </summary>
    public const long SuperAdminUserId = 1;
    /// <summary>
    /// 系统默认内置租户管理员用户ID
    /// </summary>
    public const long DefaultTenantAdminUserId = 2;

    /// <summary>
    /// 超级管理员角色ID
    /// </summary>
    public const long SuperAdminRoleId = 1000000;
    /// <summary>
    /// 系统管理员角色编码
    /// </summary>
    public const string SystemAdminRoleCode = "SYSTEM_ADMIN";

    /// <summary>
    /// 顶级菜单ID
    /// </summary>
    public const long TopMenuId = 0;
    /// <summary>
    /// 顶级菜单名称
    /// </summary>
    public const string TopMenuName = "顶级菜单";

    /// <summary>
    /// 顶级部门ID
    /// </summary>
    public const long TopDepartmentId = 0;
    /// <summary>
    /// 顶级部门名称
    /// </summary>
    public const string TopDepartmentName = "顶级部门";

    /// <summary>
    /// 租户管理菜单ID
    /// </summary>
    public const long TenantMenuId = 8000001;
}