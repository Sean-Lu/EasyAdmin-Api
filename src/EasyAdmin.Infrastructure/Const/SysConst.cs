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
    /// 系统默认内置租户编码
    /// </summary>
    public const string DefaultTenantCode = "default";

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
    /// 超级管理员角色编码
    /// </summary>
    public const string SuperAdminRoleCode = "SUPER_ADMIN";
    /// <summary>
    /// 系统管理员角色编码
    /// </summary>
    public const string SystemAdminRoleCode = "SYSTEM_ADMIN";

    /// <summary>
    /// 普通用户角色ID
    /// </summary>
    public const long NormalUserRoleId = 1000006;
    /// <summary>
    /// 普通用户角色编码
    /// </summary>
    public const string NormalUserRoleCode = "NORMAL_USER";
    /// <summary>
    /// 普通用户基础菜单ID
    /// </summary>
    public static readonly long[] NormalUserMenuIds =
    {
        1000000,// 首页
        2000000,// 数据大屏
        8100001, 8100002, 8100003, 8100004, 8100005, 8100006, 8100007, 8100008, 8100009,// 个人中心
        10000001, 10000002, 10000003,// 外部链接
        11000002, 11000003// 工具
    };

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