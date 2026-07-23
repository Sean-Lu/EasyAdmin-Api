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
using MapsterMapper;
using Sean.Utility.Security.Provider;

namespace EasyAdmin.Application.Services;

public class TenantService(
    ILogger<TenantService> logger,
    IMapper mapper,
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IParamRepository paramRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IRoleMenuRepository roleMenuRepository
    ) : ITenantService
{
    public async Task<bool> AddAsync(TenantDto dto)
    {
        ValidateValidity(dto.StartTime, dto.ExpireTime);
        dto.Code = dto.Code?.Trim();
        if (string.IsNullOrEmpty(dto.Code) || dto.Code.Length > TenantLoginPolicy.MaxTenantCodeLength)
        {
            throw new ExplicitException("租户编码长度必须为1到50个字符");
        }
        if (string.IsNullOrEmpty(dto.AdminUserName))
        {
            throw new ExplicitException("租管账号名称不能为空");
        }

        var existingTenants = (await tenantRepository.QueryAsync(entity => !entity.IsDelete))?.ToList() ?? [];
        if (existingTenants.Any(entity => string.Equals(entity.Code, dto.Code, StringComparison.Ordinal)))
        {
            throw new ExplicitException("租户编码已存在");
        }

        var tenantEntity = mapper.Map<TenantEntity>(dto);
        var adminUserEntity = new UserEntity
        {
            UserName = dto.AdminUserName,
            NickName = "管理员",
            State = CommonState.Enable
        };
        var paramEntity = await paramRepository.GetAsync(c => c.ParamKey == ConfigConst.TenantAdminInitPassword && c.State == CommonState.Enable);
        var tenantAdminInitPassword = paramEntity?.ParamValue;
        if (!string.IsNullOrEmpty(tenantAdminInitPassword))
        {
            var hash = new HashCryptoProvider();
            adminUserEntity.Password = hash.MD5(tenantAdminInitPassword).ToLower();
        }
        var adminRole = new RoleEntity
        {
            Name = "系统管理员",
            Code = SysConst.SystemAdminRoleCode,
            Description = "系统管理员角色，拥有租户内所有系统权限",
            Sort = 0,
            State = CommonState.Enable
        };
        var normalUserRole = new RoleEntity
        {
            Name = "普通用户",
            Code = SysConst.NormalUserRoleCode,
            Description = "普通用户角色，拥有基础菜单权限",
            Sort = 1,
            State = CommonState.Enable
        };
        return await tenantRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            if (!await tenantRepository.AddAsync(tenantEntity, transaction: transaction))
                throw new ExplicitException("新增租户失败");

            adminUserEntity.TenantId = tenantEntity.Id;
            if (!await userRepository.AddAsync(adminUserEntity, transaction: transaction))
                throw new ExplicitException("创建租管账号失败");

            tenantEntity.AdminUserId = adminUserEntity.Id;
            if (await tenantRepository.UpdateAsync(tenantEntity, entity => entity.AdminUserId, transaction: transaction) < 1)
                throw new ExplicitException("关联租管账号失败");

            adminRole.TenantId = tenantEntity.Id;
            if (!await roleRepository.AddAsync(adminRole, transaction: transaction))
                throw new ExplicitException("创建系统管理员角色失败");

            normalUserRole.TenantId = tenantEntity.Id;
            if (!await roleRepository.AddAsync(normalUserRole, transaction: transaction))
                throw new ExplicitException("创建普通用户角色失败");

            var normalUserRoleMenus = SysConst.NormalUserMenuIds.Select(menuId => new RoleMenuEntity
            {
                TenantId = tenantEntity.Id,
                RoleId = normalUserRole.Id,
                MenuId = menuId
            }).ToList();
            if (!await roleMenuRepository.AddAsync(normalUserRoleMenus, transaction: transaction))
                throw new ExplicitException("创建普通用户菜单权限失败");

            var userRole = new UserRoleEntity
            {
                UserId = adminUserEntity.Id,
                RoleId = adminRole.Id,
                TenantId = tenantEntity.Id
            };
            if (!await userRoleRepository.AddAsync(userRole, transaction: transaction))
                throw new ExplicitException("关联管理员角色失败");

            return true;
        });
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

    public async Task<bool> UpdateAsync(TenantUpdateDto dto)
    {
        ValidateValidity(dto.StartTime, dto.ExpireTime);
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

    public async Task<TenantEntity?> GetEnabledByCodeAsync(string code)
    {
        var candidates = (await tenantRepository.QueryAsync(entity => entity.Code == code && entity.State == CommonState.Enable && !entity.IsDelete))?.ToList() ?? [];
        return candidates.FirstOrDefault(entity => string.Equals(entity.Code, code, StringComparison.Ordinal));
    }

    private static void ValidateValidity(DateTime? startTime, DateTime? expireTime)
    {
        if (startTime.HasValue && expireTime.HasValue && startTime.Value >= expireTime.Value)
        {
            throw new ExplicitException("生效时间必须早于到期时间");
        }
    }
}