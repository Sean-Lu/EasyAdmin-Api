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
    IParamRepository paramRepository
    ) : ITenantService
{
    public async Task<bool> AddAsync(TenantDto dto)
    {
        if (string.IsNullOrEmpty(dto.AdminUserName))
        {
            throw new ExplicitException("租管账号名称不能为空");
        }

        var entity = mapper.Map<TenantEntity>(dto);
        if (!await tenantRepository.AddAsync(entity))
        {
            throw new ExplicitException("新增租户失败");
        }

        // 创建租管账号
        var userEntity = new UserEntity
        {
            UserName = entity.AdminUserName,
            NickName = "管理员",
            Role = UserRole.Administrator,
            TenantId = entity.Id
        };
        var paramEntity = await paramRepository.GetAsync(c => c.ParamKey == ConfigConst.TenantAdminInitPassword && c.State == CommonState.Enable);
        var tenantAdminInitPassword = paramEntity?.ParamValue;
        if (!string.IsNullOrEmpty(tenantAdminInitPassword))
        {
            var hash = new HashCryptoProvider();
            userEntity.Password = hash.MD5(tenantAdminInitPassword).ToLower();
        }
        if (!await userRepository.AddAsync(userEntity))
        {
            throw new ExplicitException("创建租管账号失败");
        }
        entity.AdminUserId = userEntity.Id;
        if (!(await tenantRepository.UpdateAsync(entity, c => c.AdminUserId) > 0))
        {
            throw new ExplicitException("更新租管账号失败");
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
        var entity = mapper.Map<TenantEntity>(dto);
        return await tenantRepository.UpdateAsync(entity) > 0;
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