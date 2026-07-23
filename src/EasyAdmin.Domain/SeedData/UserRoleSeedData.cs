using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 用户角色种子数据
/// </summary>
public class UserRoleSeedData : IEntitySeedData<UserRoleEntity>
{
    public IEnumerable<UserRoleEntity> SeedData()
    {
        return new[]
        {
            new UserRoleEntity { Id = 1, TenantId = SysConst.DefaultTenantId, UserId = SysConst.SuperAdminUserId, RoleId = SysConst.SuperAdminRoleId },
            new UserRoleEntity { Id = 2, TenantId = SysConst.DefaultTenantId, UserId = SysConst.DefaultTenantAdminUserId, RoleId = 1000001 },
        };
    }
}