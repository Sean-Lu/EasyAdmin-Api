using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using EasyAdmin.Infrastructure.Wrapper;
using Sean.Utility.Security.Provider;

namespace EasyAdmin.Application.Services;

public class TenantService(
    ILogger<TenantService> logger,
    IMapper mapper,
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IParamRepository paramRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository
    ) : ITenantService
{
    public async Task<bool> AddAsync(TenantDto dto)
    {
        if (string.IsNullOrEmpty(dto.AdminUserName))
        {
            throw new ExplicitException("租管账号名称不能为空");
        }

        // 1. 新增租户
        var tenantEntity = mapper.Map<TenantEntity>(dto);
        if (!await tenantRepository.AddAsync(tenantEntity))
        {
            throw new ExplicitException("新增租户失败");
        }

        // 2. 为新租户创建租管账号(租户管理员)
        var adminUserEntity = new UserEntity
        {
            UserName = dto.AdminUserName,
            NickName = "管理员",
            TenantId = tenantEntity.Id
        };
        var paramEntity = await paramRepository.GetAsync(c => c.ParamKey == ConfigConst.TenantAdminInitPassword && c.State == CommonState.Enable);
        var tenantAdminInitPassword = paramEntity?.ParamValue;
        if (!string.IsNullOrEmpty(tenantAdminInitPassword))
        {
            var hash = new HashCryptoProvider();
            adminUserEntity.Password = hash.MD5(tenantAdminInitPassword).ToLower();
        }
        if (!await userRepository.AddAsync(adminUserEntity))
        {
            throw new ExplicitException("创建租管账号失败");
        }

        // 3. 关联租管账号
        tenantEntity.AdminUserId = adminUserEntity.Id;
        if (!(await tenantRepository.UpdateAsync(tenantEntity, c => c.AdminUserId) > 0))
        {
            throw new ExplicitException("关联租管账号失败");
        }

        // 4. 为新租户创建系统管理员角色
        var adminRole = new RoleEntity
        {
            TenantId = tenantEntity.Id,
            Name = "系统管理员",
            Code = SysConst.SystemAdminRoleCode,
            Description = "系统管理员角色，拥有租户内所有系统权限",
            Sort = 0,
            State = CommonState.Enable
        };
        if (!await roleRepository.AddAsync(adminRole))
        {
            throw new ExplicitException("创建系统管理员角色失败");
        }

        // 5. 关联租管账号到管理员角色
        var userRole = new UserRoleEntity
        {
            UserId = adminUserEntity.Id,
            RoleId = adminRole.Id,
            TenantId = tenantEntity.Id
        };
        if (!await userRoleRepository.AddAsync(userRole))
        {
            throw new ExplicitException("关联管理员角色失败");
        }

        return true;
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        if (id == SysConst.DefaultTenantId)
        {
            throw new ExplicitException("不允许删除系统默认内置租户");
        }
        return await tenantRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        if (ids.Contains(SysConst.DefaultTenantId))
        {
            throw new ExplicitException("不允许删除系统默认内置租户");
        }
        return await tenantRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(TenantDto dto)
    {
        return await tenantRepository.UpdateByDtoAsync(dto, mapper.Map<TenantEntity>) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        if (id == SysConst.DefaultTenantId && state == CommonState.Disable)
        {
            throw new ExplicitException("不允许禁用系统默认内置租户");
        }
        return await tenantRepository.UpdateAsync(new TenantEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<TenantEntity>> PageAsync(TenantPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<TenantEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<TenantEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await tenantRepository.PageQueryAsync(WhereExpressionUtil.Create<TenantEntity>(entity => !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<TenantEntity> GetByIdAsync(long id)
    {
        return await tenantRepository.GetByIdAsync(id);
    }

    public async Task<TenantEntity> GetByNameAsync(string name)
    {
        return await tenantRepository.GetAsync(entity => entity.Name == name);
    }
}