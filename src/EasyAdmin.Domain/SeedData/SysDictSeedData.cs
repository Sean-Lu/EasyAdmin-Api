using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 字典种子数据
/// </summary>
public class SysDictSeedData : IEntitySeedData<SysDictEntity>
{
    public IEnumerable<SysDictEntity> SeedData()
    {
        return new[]
        {
            new SysDictEntity { Id = 1, PId = 0, Code = "sex", DictKey = -1, DictValue = "性别", Sort = 1, State = CommonState.Enable },
            new SysDictEntity { Id = 2, PId = 1, Code = "sex", DictKey = 1, DictValue = "男", Sort = 1, State = CommonState.Enable },
            new SysDictEntity { Id = 3, PId = 1, Code = "sex", DictKey = 2, DictValue = "女", Sort = 2, State = CommonState.Enable },

            new SysDictEntity { Id = 4, PId = 0, Code = "political_status", DictKey = -1, DictValue = "政治面貌", Sort = 1, State = CommonState.Enable },
            new SysDictEntity { Id = 5, PId = 4, Code = "political_status", DictKey = 1, DictValue = "中共党员", Sort = 1, State = CommonState.Enable },
            new SysDictEntity { Id = 6, PId = 4, Code = "political_status", DictKey = 2, DictValue = "中共预备党员", Sort = 2, State = CommonState.Enable },
            new SysDictEntity { Id = 7, PId = 4, Code = "political_status", DictKey = 3, DictValue = "共青团员", Sort = 3, State = CommonState.Enable },
            new SysDictEntity { Id = 8, PId = 4, Code = "political_status", DictKey = 4, DictValue = "群众", Sort = 4, State = CommonState.Enable },
        };
    }
}