using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Extensions;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Helper;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class RegionService(
    ILogger<RegionService> logger,
    IMapper mapper,
    IRegionRepository regionRepository
    ) : IRegionService
{
    public async Task<bool> AddAsync(RegionDto dto)
    {
        var entity = mapper.Map<RegionEntity>(dto);
        return await regionRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        // 检查是否有子级
        var hasChildren = await regionRepository.ExistsAsync(entity => entity.PId == id && !entity.IsDelete);
        if (hasChildren)
        {
            throw new ExplicitException("该行政区划下存在子级行政区划，无法删除");
        }

        return await regionRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        // 检查是否有子级
        var hasChildren = await regionRepository.ExistsAsync(entity => ids.Contains(entity.PId) && !entity.IsDelete && !ids.Contains(entity.Id));
        if (hasChildren)
        {
            throw new ExplicitException("所选行政区划中存在包含子级行政区划的记录，无法删除");
        }

        return await regionRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(RegionUpdateDto dto)
    {
        return await regionRepository.UpdateByDtoAsync(dto, mapper.Map<RegionEntity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await regionRepository.UpdateAsync(new RegionEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<List<RegionEntity>> GetRegionTreeAsync(RegionListReqDto request)
    {
        var list = (await regionRepository.QueryAsync(WhereExpressionUtil.Create<RegionEntity>(entity => !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
            .AndAlsoIF(request.Level.HasValue, entity => entity.Level == request.Level.Value)
            .AndAlsoIF(!request.All, entity => entity.State == CommonState.Enable)))?.ToList() ?? new List<RegionEntity>();
        if (list.Any() && !list.Exists(c => c.PId == 0))
        {
            // 向上查找所有上级行政区划
            await TreeHelper.AddAllParentsAsync(
                list,
                async (id) => await regionRepository.GetByIdAsync(id),
                entity => entity.Id,
                entity => entity.PId,
                entity => entity.PId == 0
            );
        }

        return list.ToTreeList(0);
    }

    public async Task<RegionEntity> GetByIdAsync(long id)
    {
        var entity = await regionRepository.GetByIdAsync(id);
        if (entity.Id > 0)
        {
            entity.ParentFullPath = await GetParentFullPathAsync(entity);
        }

        return entity;
    }

    private async Task<string> GetParentFullPathAsync(RegionEntity entity)
    {
        if (entity.PId == 0)
        {
            return string.Empty;
        }

        if (entity.PId == entity.Id)
        {
            return entity.Name;
        }

        var parent = await regionRepository.GetByIdAsync(entity.PId);
        var parentFullPath = await GetParentFullPathAsync(parent);
        return string.IsNullOrEmpty(parentFullPath) ? parent.Name : $"{parentFullPath} / {parent.Name}";
    }
}
