using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 租户种子数据
/// </summary>
public class TenantSeedData : IEntitySeedData<TenantEntity>
{
    public IEnumerable<TenantEntity> SeedData()
    {
        var defaultTenantAdminUserId = new UserSeedData().SeedData().First(c => c is { TenantId: SysConst.DefaultTenantId, Role: UserRole.Administrator }).Id;
        return new[]
        {
            new TenantEntity { Id = SysConst.DefaultTenantId, Name = "系统默认", AdminUserId = defaultTenantAdminUserId, Remark = "系统默认内置租户", State = CommonState.Enable }
        };
    }
}