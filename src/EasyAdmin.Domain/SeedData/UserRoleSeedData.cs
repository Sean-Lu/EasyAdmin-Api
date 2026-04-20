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
            // 超级管理员角色
            new UserRoleEntity { Id = 1, TenantId = SysConst.DefaultTenantId, UserId = SysConst.SuperAdminUserId, RoleId = SysConst.SuperAdminRoleId },
            
            // 系统管理员角色
            new UserRoleEntity { Id = 2, TenantId = SysConst.DefaultTenantId, UserId = SysConst.DefaultTenantAdminUserId, RoleId = 1000001 },
            new UserRoleEntity { Id = 3, TenantId = SysConst.DefaultTenantId, UserId = 10, RoleId = 1000001 },
            
            // 普通员工角色
            new UserRoleEntity { Id = 4, TenantId = SysConst.DefaultTenantId, UserId = 3, RoleId = 1000004 },
            new UserRoleEntity { Id = 5, TenantId = SysConst.DefaultTenantId, UserId = 4, RoleId = 1000004 },
        };
    }
}