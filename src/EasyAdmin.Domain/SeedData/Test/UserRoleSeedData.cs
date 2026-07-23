using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Domain.SeedData.Test;

/// <summary>
/// 测试 - 用户角色种子数据
/// </summary>
public class UserRoleSeedData : IEntitySeedData<UserRoleEntity>, ITestSeedData
{
    public IEnumerable<UserRoleEntity> SeedData()
    {
        return new[]
        {
            new UserRoleEntity { Id = 4, TenantId = SysConst.DefaultTenantId, UserId = 3, RoleId = SysConst.NormalUserRoleId },
            new UserRoleEntity { Id = 5, TenantId = SysConst.DefaultTenantId, UserId = 4, RoleId = SysConst.NormalUserRoleId },
            new UserRoleEntity { Id = 6, TenantId = SysConst.DefaultTenantId, UserId = 10, RoleId = 1000001 },
        };
    }
}