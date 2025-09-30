using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Enums;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class MenuService(
    ILogger<MenuService> logger,
    IMapper mapper,
    IMenuRepository menuRepository
    ) : IMenuService
{
    private const string TopMenuName = "顶级菜单";

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
        var entity = mapper.Map<MenuEntity>(dto);
        return await menuRepository.UpdateAsync(entity) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await menuRepository.UpdateAsync(new MenuEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<List<MenuEntity>?> GetMenuTreeAsync(MenuListReqDto request)
    {
        var menuTree = (await menuRepository.QueryAsync(WhereExpressionUtil.Create<MenuEntity>(entity => !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Title), entity => entity.Title.Contains(request.Title))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Path), entity => entity.Path.Contains(request.Path))
            .AndAlsoIF(!request.All, entity => entity.State == CommonState.Enable)))?.ToList().ToTreeList();
        if (request.IncludeTopMenu)
        {
            return new List<MenuEntity>
            {
                new()
                {
                    Id = 0,
                    PId = 0,
                    Title = TopMenuName,
                    Children = menuTree
                }
            };
        }
        return menuTree;
    }

    public async Task<MenuEntity> GetByIdAsync(long id)
    {
        var menu = await menuRepository.GetByIdAsync(id);
        if (menu.Id > 0)
        {
            menu.ParentFullPath = await GetParentMenuFullPathAsync(menu);
        }
        return menu;
    }

    private async Task<string> GetParentMenuFullPathAsync(MenuEntity menu)
    {
        if (menu.PId == 0)
        {
            return TopMenuName;
        }
        if (menu.PId == menu.Id)
        {
            return menu.Title;
        }

        var parentMenu = await menuRepository.GetByIdAsync(menu.PId);
        var parentMenuFullPath = await GetParentMenuFullPathAsync(parentMenu);
        return $"{parentMenuFullPath} / {parentMenu.Title}";
    }
}