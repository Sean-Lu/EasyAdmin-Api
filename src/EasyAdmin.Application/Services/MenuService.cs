using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Helper;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class MenuService(
    ILogger<MenuService> logger,
    IMapper mapper,
    IMenuRepository menuRepository,
    IUserRoleRepository userRoleRepository,
    IRoleRepository roleRepository,
    IRoleMenuRepository roleMenuRepository
    ) : IMenuService
{
    /// <summary>
    /// 系统管理员角色没有权限的菜单ID列表
    /// </summary>
    private static readonly List<long> SystemAdminRoleIgnoreMenuIds = new() { SysConst.TenantMenuId };

    public async Task<bool> AddAsync(MenuDto dto)
    {
        var entity = mapper.Map<MenuEntity>(dto);
        return await menuRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await menuRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await menuRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(MenuDto dto)
    {
        return await menuRepository.UpdateByDtoAsync(dto, mapper.Map<MenuEntity>) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await menuRepository.UpdateAsync(new MenuEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<List<MenuEntity>> GetMenuTreeAsync(long userId, MenuListReqDto request)
    {
        // 获取用户的所有角色ID
        var userRoleIds = await userRoleRepository.GetUserRoleIdsAsync(userId);
        // 检查是否有超级管理员角色，超级管理员默认拥有所有菜单权限
        var hasSuperAdminRole = userRoleIds.Contains(SysConst.SuperAdminRoleId);
        // 检查是否有系统管理员角色，系统管理员拥有大部分系统管理权限（不包含租户配置）
        var hasSystemAdminRole = await roleRepository.ExistsAsync(entity => userRoleIds.Contains(entity.Id) && entity.Code == SysConst.SystemAdminRoleCode && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete);
        // 获取普通用户拥有权限的所有菜单ID
        var menuIds = new List<long>();
        if (!hasSuperAdminRole && !hasSystemAdminRole)
        {
            // 普通用户权限处理逻辑
            foreach (var roleId in userRoleIds)
            {
                var roleMenuIds = (await roleMenuRepository.QueryAsync(entity => entity.RoleId == roleId, fieldExpression: entity => entity.MenuId))?.ToList();
                if (roleMenuIds != null && roleMenuIds.Any())
                {
                    menuIds.AddRange(roleMenuIds.Select(rm => rm.MenuId));
                }
            }
            menuIds = menuIds.Distinct().ToList();
            if (!menuIds.Any())// 当前用户未分配任何菜单权限
            {
                return new List<MenuEntity>();
            }
        }

        var list = (await menuRepository.QueryAsync(WhereExpressionUtil.Create<MenuEntity>(entity => !entity.IsDelete)
                .AndAlsoIF(!hasSuperAdminRole && !hasSystemAdminRole, entity => menuIds.Contains(entity.Id))
                .AndAlsoIF(!hasSuperAdminRole && hasSystemAdminRole, entity => !SystemAdminRoleIgnoreMenuIds.Contains(entity.Id))
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Title), entity => entity.Title.Contains(request.Title))
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Path), entity => entity.Path.Contains(request.Path))
                .AndAlsoIF(!request.All, entity => entity.State == CommonState.Enable)))?.ToList() ?? new List<MenuEntity>();
        if (list.Any())
        {
            // 向上查找所有上级菜单
            await TreeHelper.AddAllParentsAsync(
                list,
                async (id) => await menuRepository.GetByIdAsync(id),
                entity => entity.Id,
                entity => entity.PId,
                entity => entity.PId == SysConst.TopMenuId
            );
        }

        var treeList = list.ToTreeList(SysConst.TopMenuId);
        if (request.IncludeTopMenu)
        {
            return new List<MenuEntity>
            {
                new()
                {
                    Id = 0,
                    PId = SysConst.TopMenuId,
                    Title = SysConst.TopMenuName,
                    Children = treeList
                }
            };
        }
        return treeList ?? new List<MenuEntity>();
    }

    public async Task<MenuEntity> GetByIdAsync(long id)
    {
        var entity = await menuRepository.GetByIdAsync(id);
        if (entity.Id > 0)
        {
            entity.ParentFullPath = await GetParentFullPathAsync(entity);
        }
        return entity;
    }

    private async Task<string> GetParentFullPathAsync(MenuEntity entity)
    {
        if (entity.PId == 0)
        {
            return SysConst.TopMenuName;
        }
        if (entity.PId == entity.Id)
        {
            return entity.Title;
        }

        var parent = await menuRepository.GetByIdAsync(entity.PId);
        var parentFullPath = await GetParentFullPathAsync(parent);
        return $"{parentFullPath} / {parent.Title}";
    }
}