using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData.Test;

/// <summary>
/// 测试 - 字典类型种子数据
/// </summary>
public class SysDictTypeSeedData : IEntitySeedData<SysDictTypeEntity>, ITestSeedData
{
    public IEnumerable<SysDictTypeEntity> SeedData()
    {
        return new[]
        {
            new SysDictTypeEntity { Id = 1, TenantId = SysConst.DefaultTenantId, Name = "性别", Code = "sex", Sort = 1, State = CommonState.Enable },
            new SysDictTypeEntity { Id = 2, TenantId = SysConst.DefaultTenantId, Name = "政治面貌", Code = "political_status", Sort = 2, State = CommonState.Enable },
        };
    }
}