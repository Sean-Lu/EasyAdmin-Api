using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.SeedData;

/// <summary>
/// 角色种子数据
/// </summary>
public class RoleSeedData : IEntitySeedData<RoleEntity>
{
    public IEnumerable<RoleEntity> SeedData()
    {
        return new[]
        {
            new RoleEntity 
            { 
                Id = 1000001, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "系统管理员", 
                Code = "SYSTEM_ADMIN", 
                Description = "系统管理员角色，拥有大部分系统管理权限", 
                Sort = 1, 
                State = CommonState.Enable 
            },
            new RoleEntity 
            { 
                Id = 1000002, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "人事专员", 
                Code = "HR_STAFF", 
                Description = "人事专员角色，负责人事相关操作", 
                Sort = 2, 
                State = CommonState.Enable 
            },
            new RoleEntity 
            { 
                Id = 1000003, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "财务人员", 
                Code = "FINANCE_STAFF", 
                Description = "财务人员角色，负责财务相关操作", 
                Sort = 3, 
                State = CommonState.Enable 
            },
            new RoleEntity 
            { 
                Id = 1000004, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "普通员工", 
                Code = "NORMAL_EMPLOYEE", 
                Description = "普通员工角色，基础权限", 
                Sort = 4, 
                State = CommonState.Enable 
            },
            new RoleEntity 
            { 
                Id = 1000005, 
                TenantId = SysConst.DefaultTenantId, 
                Name = "访客", 
                Code = "GUEST", 
                Description = "访客角色，仅查看权限", 
                Sort = 5, 
                State = CommonState.Enable 
            }
        };
    }
}