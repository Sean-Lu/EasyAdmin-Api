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
        return new[]
        {
            new TenantEntity { Id = SysConst.DefaultTenantId, Name = "系统默认", AdminUserId = SysConst.DefaultTenantAdminUserId, Remark = "系统默认内置租户", State = CommonState.Enable }
        };
    }
}