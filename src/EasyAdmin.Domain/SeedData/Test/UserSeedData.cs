using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using Sean.Utility.Security.Provider;

namespace EasyAdmin.Domain.SeedData.Test;

/// <summary>
/// 测试 - 用户种子数据
/// </summary>
public class UserSeedData : IEntitySeedData<UserEntity>, ITestSeedData
{
    public IEnumerable<UserEntity> SeedData()
    {
        var hash = new HashCryptoProvider();
        var defaultPwd = hash.MD5("123456").ToLower();
        return new[]
        {
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = 3, UserName = "user01", Password = defaultPwd, NickName = "普通用户01", State = CommonState.Enable },
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = 4, UserName = "user02", Password = defaultPwd, NickName = "普通用户02", State = CommonState.Enable },
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = 10, UserName = "Sean", Password = defaultPwd, NickName = "大师兄", Email = "weixian.lu@foxmail.com", DepartmentId = 1000, PositionId = 1, State = CommonState.Enable },
        };
    }
}