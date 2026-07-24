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
            new ParamEntity{ Id = 1, ParamName = "是否开启多租户功能", ParamKey = ConfigConst.TenantEnable, ParamValue = "false", ValueType = ParamValueType.Boolean, Remark = "控制登录和注册时是否启用租户功能", Sort = 1, State = CommonState.Enable },
            new ParamEntity{ Id = 2, ParamName = "租管账号初始密码", ParamKey = ConfigConst.TenantAdminInitPassword, ParamValue = "admin", ValueType = ParamValueType.String, Remark = "创建新租户时用于设置租户管理员初始密码", Sort = 2, State = CommonState.Enable },
            new ParamEntity{ Id = 3, ParamName = "是否开启注册功能", ParamKey = ConfigConst.UserEnableRegister, ParamValue = "false", ValueType = ParamValueType.Boolean, Remark = "控制是否允许用户注册账号", Sort = 3, State = CommonState.Enable },
            new ParamEntity{ Id = 6, ParamName = "新注册用户是否需要审核", ParamKey = ConfigConst.UserRegisterNeedApproval, ParamValue = "false", ValueType = ParamValueType.Boolean, Remark = "控制新注册用户是否需要管理员审核", Sort = 4, State = CommonState.Enable },
            new ParamEntity{ Id = 4, ParamName = "账号初始密码", ParamKey = ConfigConst.UserInitPassword, ParamValue = "123456", ValueType = ParamValueType.String, Remark = "后台新增用户或重置密码时使用的初始密码", Sort = 5, State = CommonState.Enable },
            new ParamEntity{ Id = 5, ParamName = "账号密码错误锁定次数", ParamKey = ConfigConst.UserPasswordMismatchLockCount, ParamValue = "5", ValueType = ParamValueType.Number, Remark = "密码连续验证失败达到此次数后暂时禁止继续验证，设置为0或负数关闭锁定", Sort = 6, State = CommonState.Enable },
            new ParamEntity{ Id = 7, ParamName = "账号密码错误锁定时间", ParamKey = ConfigConst.UserPasswordMismatchLockMinutes, ParamValue = "1", ValueType = ParamValueType.Number, Remark = "密码错误次数缓存的有效时间，单位为分钟", Sort = 7, State = CommonState.Enable },
        };
    }
}