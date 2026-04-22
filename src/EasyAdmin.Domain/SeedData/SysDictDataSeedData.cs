using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

public class SysDictDataSeedData : IEntitySeedData<SysDictDataEntity>
{
    public IEnumerable<SysDictDataEntity> SeedData()
    {
        return new[]
        {
            new SysDictDataEntity { Id = 1, TenantId = SysConst.DefaultTenantId, DictTypeId = 1, DictKey = 1, DictValue = "男", Sort = 1, State = CommonState.Enable },
            new SysDictDataEntity { Id = 2, TenantId = SysConst.DefaultTenantId, DictTypeId = 1, DictKey = 2, DictValue = "女", Sort = 2, State = CommonState.Enable },
            new SysDictDataEntity { Id = 3, TenantId = SysConst.DefaultTenantId, DictTypeId = 2, DictKey = 1, DictValue = "中共党员", Sort = 1, State = CommonState.Enable },
            new SysDictDataEntity { Id = 4, TenantId = SysConst.DefaultTenantId, DictTypeId = 2, DictKey = 2, DictValue = "中共预备党员", Sort = 2, State = CommonState.Enable },
            new SysDictDataEntity { Id = 5, TenantId = SysConst.DefaultTenantId, DictTypeId = 2, DictKey = 3, DictValue = "共青团员", Sort = 3, State = CommonState.Enable },
            new SysDictDataEntity { Id = 6, TenantId = SysConst.DefaultTenantId, DictTypeId = 2, DictKey = 4, DictValue = "群众", Sort = 4, State = CommonState.Enable },
        };
    }
}
