using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Helper;
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
    private const long TopMenuId = 0;
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
        return await menuRepository.UpdateByDtoAsync(dto, mapper.Map<MenuEntity>) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await menuRepository.UpdateAsync(new MenuEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<List<MenuEntity>?> GetMenuTreeAsync(MenuListReqDto request)
    {
        var list = (await menuRepository.QueryAsync(WhereExpressionUtil.Create<MenuEntity>(entity => !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Title), entity => entity.Title.Contains(request.Title))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Path), entity => entity.Path.Contains(request.Path))
            .AndAlsoIF(!request.All, entity => entity.State == CommonState.Enable)))?.ToList() ?? new List<MenuEntity>();
        if (list.Any() && !list.Exists(c => c.PId == TopMenuId))
        {
            // 向上查找所有上级菜单
            await TreeHelper.AddAllParentsAsync(
                list,
                async (id) => await menuRepository.GetByIdAsync(id),
                entity => entity.Id,
                entity => entity.PId,
                entity => entity.PId == TopMenuId
            );
        }
        var treeList = list.ToTreeList(TopMenuId);
        if (request.IncludeTopMenu)
        {
            return new List<MenuEntity>
            {
                new()
                {
                    Id = 0,
                    PId = TopMenuId,
                    Title = TopMenuName,
                    Children = treeList
                }
            };
        }
        return treeList;
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
            return TopMenuName;
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