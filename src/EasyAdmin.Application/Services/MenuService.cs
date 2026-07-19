using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Helper;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
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
        entity.TenantId = TenantContextHolder.TenantId;
        MenuRules.NormalizeAndValidate(entity);
        await ValidateParentAsync(entity.PId, entity.TenantId);
        await ValidateRouteUniqueAsync(entity);
        return await menuRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        await ValidateMenusCanDeleteAsync(new[] { id });
        return await menuRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        await ValidateMenusCanDeleteAsync(ids);
        return await menuRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(MenuUpdateDto dto)
    {
        var stored = await menuRepository.GetByIdAsync(dto.Id);
        EnsureCanModify(stored);

        var entity = mapper.Map<MenuEntity>(dto);
        entity.Id = dto.Id;
        entity.TenantId = stored.TenantId;
        MenuRules.NormalizeAndValidate(entity);
        await ValidateParentAsync(entity.PId, entity.TenantId, entity.Id);
        await ValidateRouteUniqueAsync(entity);
        if (entity.Type != MenuType.Directory && await HasChildrenAsync(new[] { entity.Id }))
        {
            throw new ExplicitException("存在下级菜单，不能修改为非目录类型");
        }

        var entityId = entity.Id;
        return await menuRepository.UpdateAsync(
            entity,
            menu => new
            {
                menu.PId,
                menu.Sort,
                menu.Type,
                menu.Icon,
                menu.Title,
                menu.Path,
                menu.OutLink,
                menu.OutLinkOpenType,
                menu.State
            },
            menu => menu.Id == entityId) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        EnsureCanModify(await menuRepository.GetByIdAsync(id));
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
                var roleMenuIds = (await roleMenuRepository.QueryAsync(entity => entity.RoleId == roleId && entity.TenantId == TenantContextHolder.TenantId, fieldExpression: entity => entity.MenuId))?.ToList();
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

        var tenantId = TenantContextHolder.TenantId;
        var list = (await menuRepository.QueryAsync(WhereExpressionUtil.Create<MenuEntity>(entity => !entity.IsDelete && (entity.TenantId == null || entity.TenantId == tenantId))
                .AndAlsoIF(!hasSuperAdminRole && !hasSystemAdminRole, entity => menuIds.Contains(entity.Id))
                .AndAlsoIF(!hasSuperAdminRole && hasSystemAdminRole, entity => !SystemAdminRoleIgnoreMenuIds.Contains(entity.Id))
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Title), entity => entity.Title.Contains(request.Title!))
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Path), entity => entity.Path != null && entity.Path.Contains(request.Path!))
                .AndAlsoIF(!request.All, entity => entity.State == CommonState.Enable)))?.ToList() ?? new List<MenuEntity>();
        if (list.Any())
        {
            // 向上查找所有上级菜单
            await TreeHelper.AddAllParentsAsync(
                list,
                async (id) => await menuRepository.GetAsync(entity =>
                    entity.Id == id &&
                    !entity.IsDelete &&
                    (entity.TenantId == null || entity.TenantId == tenantId)),
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
                    Type = MenuType.Directory,
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
        EnsureVisible(entity);
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
        EnsureVisible(parent);
        var parentFullPath = await GetParentFullPathAsync(parent);
        return $"{parentFullPath} / {parent.Title}";
    }

    private async Task ValidateParentAsync(long parentId, long? menuTenantId, long? menuId = null)
    {
        if (parentId == SysConst.TopMenuId)
        {
            return;
        }

        var visited = new HashSet<long>();
        var currentId = parentId;
        while (currentId != SysConst.TopMenuId)
        {
            if (menuId.HasValue && currentId == menuId.Value)
            {
                throw new ExplicitException("上级菜单不能选择自身或下级菜单");
            }
            if (!visited.Add(currentId))
            {
                throw new ExplicitException("菜单层级存在循环");
            }

            var parent = await menuRepository.GetByIdAsync(currentId);
            var canUseParent = parent != null &&
                               !parent.IsDelete &&
                               parent.Type == MenuType.Directory &&
                               (menuTenantId == null
                                   ? parent.TenantId == null
                                   : parent.TenantId == null || parent.TenantId == menuTenantId);
            if (!canUseParent)
            {
                throw new ExplicitException("上级菜单不存在或不是可用目录");
            }

            currentId = parent.PId;
        }
    }

    private async Task ValidateRouteUniqueAsync(MenuEntity entity)
    {
        if (entity.Type == MenuType.Directory || string.IsNullOrWhiteSpace(entity.Path))
        {
            return;
        }

        var menuId = entity.Id;
        var path = entity.Path;
        var tenantId = entity.TenantId.GetValueOrDefault();
        var exists = await menuRepository.ExistsAsync(
            WhereExpressionUtil.Create<MenuEntity>(menu =>
                    !menu.IsDelete &&
                    menu.Id != menuId &&
                    menu.Path == path)
                .AndAlsoIF(entity.TenantId.HasValue,
                    menu => menu.TenantId == null || menu.TenantId == tenantId));
        if (exists)
        {
            throw new ExplicitException($"菜单路由已存在: {entity.Path}");
        }
    }

    private async Task ValidateMenusCanDeleteAsync(IEnumerable<long> ids)
    {
        var idList = ids.Distinct().ToList();
        var menus = await menuRepository.GetByIdsAsync(idList) ?? new List<MenuEntity>();
        if (menus.Count != idList.Count)
        {
            throw new ExplicitException("菜单不存在");
        }
        foreach (var menu in menus)
        {
            EnsureCanModify(menu);
        }
        if (await HasChildrenAsync(idList))
        {
            throw new ExplicitException("请先删除下级菜单");
        }
    }

    private async Task<bool> HasChildrenAsync(IEnumerable<long> ids)
    {
        var idList = ids.ToList();
        return await menuRepository.ExistsAsync(menu => !menu.IsDelete && idList.Contains(menu.PId));
    }

    private static void EnsureVisible(MenuEntity? entity)
    {
        if (entity == null ||
            entity.IsDelete ||
            (entity.TenantId != null && entity.TenantId != TenantContextHolder.TenantId))
        {
            throw new ExplicitException("菜单不存在");
        }
    }

    private static void EnsureCanModify(MenuEntity? entity)
    {
        EnsureVisible(entity);
        if (entity!.TenantId == null
                ? TenantContextHolder.UserId != SysConst.SuperAdminUserId
                : entity.TenantId != TenantContextHolder.TenantId)
        {
            throw new ExplicitException("无权修改该菜单");
        }
    }
}