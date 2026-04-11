using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class RoleService(
    ILogger<RoleService> logger,
    IMapper mapper,
    IRoleRepository roleRepository,
    IRoleMenuRepository roleMenuRepository
    ) : IRoleService
{
    public async Task<bool> AddAsync(RoleDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name) && await roleRepository.ExistsAsync(entity => entity.Name == dto.Name && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId))
        {
            throw new ExplicitException("角色名称已存在");
        }
        if (!string.IsNullOrWhiteSpace(dto.Code) && await roleRepository.ExistsAsync(entity => entity.Code == dto.Code && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId))
        {
            throw new ExplicitException("角色编码已存在");
        }

        var entity = mapper.Map<RoleEntity>(dto);
        if (entity.Sort == 0)
        {
            entity.Sort = 1;
        }
        return await roleRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        // 删除角色前删除其所有菜单权限
        await roleMenuRepository.DeleteAsync(entity => entity.RoleId == id && entity.TenantId == TenantContextHolder.TenantId);
        return await roleRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        // 批量删除角色前删除其所有菜单权限
        foreach (var roleId in ids)
        {
            await roleMenuRepository.DeleteAsync(entity => entity.RoleId == roleId && entity.TenantId == TenantContextHolder.TenantId);
        }
        return await roleRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(RoleDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name) && await roleRepository.ExistsAsync(entity => entity.Name == dto.Name && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId && entity.Id != dto.Id))
        {
            throw new ExplicitException("角色名称已存在");
        }
        if (!string.IsNullOrWhiteSpace(dto.Code) && await roleRepository.ExistsAsync(entity => entity.Code == dto.Code && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId && entity.Id != dto.Id))
        {
            throw new ExplicitException("角色编码已存在");
        }

        return await roleRepository.UpdateByDtoAsync(dto, mapper.Map<RoleEntity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await roleRepository.UpdateAsync(new RoleEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<RoleEntity>> PageAsync(RolePageReqDto request)
    {
        var query = WhereExpressionUtil.Create<RoleEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Code), entity => entity.Code.Contains(request.Code));
        var orderBy = OrderByConditionBuilder<RoleEntity>.Build(OrderByType.Asc, entity => entity.Sort);
        orderBy.Next = OrderByConditionBuilder<RoleEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await roleRepository.PageQueryAsync(query, orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<List<RoleEntity>> GetAllRolesAsync()
    {
        var orderBy = OrderByConditionBuilder<RoleEntity>.Build(OrderByType.Asc, entity => entity.Sort);
        orderBy.Next = OrderByConditionBuilder<RoleEntity>.Build(OrderByType.Desc, entity => entity.Id);
        var roles = await roleRepository.QueryAsync(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete && entity.State == CommonState.Enable, orderBy);
        return roles?.ToList() ?? new List<RoleEntity>();
    }

    public async Task<RoleEntity> GetByIdAsync(long id)
    {
        return await roleRepository.GetByIdAsync(id);
    }

    public async Task<bool> AssignMenusToRoleAsync(RoleMenuAssignmentDto dto)
    {
        // 使用自动事务（自动提交或回滚）
        return await roleMenuRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            // 先删除该角色的所有菜单权限
            await roleMenuRepository.DeleteAsync(entity => entity.RoleId == dto.RoleId && entity.TenantId == TenantContextHolder.TenantId, transaction);

            // 添加新的菜单权限
            if (dto.MenuIds.Any())
            {
                var roleMenus = dto.MenuIds.Select(menuId => new RoleMenuEntity
                {
                    RoleId = dto.RoleId,
                    MenuId = menuId
                }).ToList();
                await roleMenuRepository.AddAsync(roleMenus, transaction: transaction);
            }

            return true;
        });
    }

    public async Task<List<long>> GetRoleMenuIdsAsync(long roleId)
    {
        var roleMenus = await roleMenuRepository.QueryAsync(entity => entity.RoleId == roleId && entity.TenantId == TenantContextHolder.TenantId, fieldExpression: entity => entity.MenuId);
        return roleMenus?.Select(rm => rm.MenuId).ToList() ?? new List<long>();
    }

    public async Task<List<RoleMenuEntity>> GetRoleMenusAsync(long roleId)
    {
        var roleMenus = await roleMenuRepository.QueryAsync(entity => entity.RoleId == roleId && entity.TenantId == TenantContextHolder.TenantId);
        return roleMenus?.ToList() ?? new List<RoleMenuEntity>();
    }
}