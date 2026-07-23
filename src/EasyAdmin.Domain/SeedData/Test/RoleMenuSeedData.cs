using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Domain.SeedData.Test;

/// <summary>
/// 测试 - 角色菜单种子数据
/// </summary>
public class RoleMenuSeedData : IEntitySeedData<RoleMenuEntity>, ITestSeedData
{
    public IEnumerable<RoleMenuEntity> SeedData()
    {
        var normalEmployeeRoleMenuIds = new long[]
        {
            1000000,// 首页
            2000000,// 数据大屏
            8100001, 8100002, 8100003, 8100004, 8100005, 8100006, 8100007, 8100008, 8100009,// 个人中心
            10000001, 10000002, 10000003,// 外部链接
            11000002, 11000003// 工具
        };
        return normalEmployeeRoleMenuIds.Select((menuId, index) => new RoleMenuEntity
        {
            Id = index + 1,
            TenantId = SysConst.DefaultTenantId,
            RoleId = 1000004,// 普通员工角色
            MenuId = menuId
        });
    }
}