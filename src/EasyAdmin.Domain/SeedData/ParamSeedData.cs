using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 参数配置种子数据
/// </summary>
public class ParamSeedData : IEntitySeedData<ParamEntity>
{
    public IEnumerable<ParamEntity> SeedData()
    {
        return new[]
        {
            new ParamEntity{ Id = 1, ParamName = "是否开启多租户功能", ParamKey = ConfigConst.TenantEnable, ParamValue = false.ToString(), State = CommonState.Enable },
            new ParamEntity{ Id = 2, ParamName = "租管账号初始密码", ParamKey = ConfigConst.TenantAdminInitPassword, ParamValue = "admin", State = CommonState.Enable },
            new ParamEntity{ Id = 3, ParamName = "是否开启注册功能", ParamKey = ConfigConst.UserEnableRegister, ParamValue = true.ToString(), State = CommonState.Enable },
            new ParamEntity{ Id = 4, ParamName = "账号初始密码", ParamKey = ConfigConst.UserInitPassword, ParamValue = "123456", State = CommonState.Enable },
            new ParamEntity{ Id = 5, ParamName = "账号密码错误锁定次数", ParamKey = ConfigConst.UserPasswordMismatchLockCount, ParamValue = "5", State = CommonState.Enable },
        };
    }
}