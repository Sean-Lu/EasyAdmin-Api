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
            new RoleEntity { Id = SysConst.SuperAdminRoleId, TenantId = SysConst.DefaultTenantId, Name = "超级管理员", Code = SysConst.SuperAdminRoleCode, Description = "超级管理员角色，拥有所有系统权限，包括租户配置", Sort = 0, State = CommonState.Enable },
            new RoleEntity { Id = 1000001, TenantId = SysConst.DefaultTenantId, Name = "系统管理员", Code = SysConst.SystemAdminRoleCode, Description = "系统管理员角色，拥有租户内所有系统权限", Sort = 1, State = CommonState.Enable }
        };
    }
}