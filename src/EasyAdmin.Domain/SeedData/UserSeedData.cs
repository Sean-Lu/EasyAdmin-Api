using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using Sean.Utility.Security.Provider;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 用户种子数据
/// </summary>
public class UserSeedData : IEntitySeedData<UserEntity>
{
    public IEnumerable<UserEntity> SeedData()
    {
        var hash = new HashCryptoProvider();
        return new[]
        {
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = SysConst.SuperAdminUserId, UserName = "superAdmin", Password = hash.MD5("superAdmin").ToLower(), NickName = "超级管理员", State = CommonState.Enable },
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = SysConst.DefaultTenantAdminUserId, UserName = "admin", Password = hash.MD5("admin").ToLower(), NickName = "管理员", State = CommonState.Enable },
        };
    }
}