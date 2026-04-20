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
        var defaultPwd = hash.MD5("123456").ToLower();
        return new[]
        {
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = SysConst.SuperAdminUserId, UserName = "superAdmin", Password = hash.MD5("superAdmin").ToLower(), NickName = "超级管理员", State = CommonState.Enable },

            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = SysConst.DefaultTenantAdminUserId, UserName = "admin", Password = hash.MD5("admin").ToLower(), NickName = "管理员", State = CommonState.Enable },
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = 3, UserName = "user01", Password = defaultPwd, NickName = "普通用户01", State = CommonState.Enable },
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = 4, UserName = "user02", Password = defaultPwd, NickName = "普通用户02", State = CommonState.Enable },
            new UserEntity { TenantId = SysConst.DefaultTenantId, Id = 10, UserName = "Sean", Password = defaultPwd, NickName = "大师兄", Email = "weixian.lu@foxmail.com", State = CommonState.Enable },
        };
    }
}