using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData.Test;

/// <summary>
/// 测试 - 角色种子数据
/// </summary>
public class RoleSeedData : IEntitySeedData<RoleEntity>, ITestSeedData
{
    public IEnumerable<RoleEntity> SeedData()
    {
        return new[]
        {
            new RoleEntity { Id = 1000002, TenantId = SysConst.DefaultTenantId, Name = "人事专员", Code = "HR_STAFF", Description = "人事专员角色，负责相关操作", Sort = 2, State = CommonState.Enable },
            new RoleEntity { Id = 1000003, TenantId = SysConst.DefaultTenantId, Name = "财务人员", Code = "FINANCE_STAFF", Description = "财务人员角色，负责相关操作", Sort = 3, State = CommonState.Enable },
            new RoleEntity { Id = 1000005, TenantId = SysConst.DefaultTenantId, Name = "访客", Code = "GUEST", Description = "访客角色，仅查看权限", Sort = 5, State = CommonState.Enable }
        };
    }
}