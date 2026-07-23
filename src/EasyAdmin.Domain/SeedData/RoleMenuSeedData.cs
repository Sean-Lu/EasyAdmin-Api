using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 角色菜单种子数据
/// </summary>
public class RoleMenuSeedData : IEntitySeedData<RoleMenuEntity>
{
    public IEnumerable<RoleMenuEntity> SeedData()
    {
        return SysConst.NormalUserMenuIds.Select((menuId, index) => new RoleMenuEntity
        {
            Id = index + 1,
            TenantId = SysConst.DefaultTenantId,
            RoleId = SysConst.NormalUserRoleId,
            MenuId = menuId
        });
    }
}
