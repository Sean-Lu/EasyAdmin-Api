using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class SysDictDataService(
    ILogger<SysDictDataService> logger,
    IMapper mapper,
    ISysDictDataRepository sysDictDataRepository,
    ISysDictTypeRepository sysDictTypeRepository
) : ISysDictDataService
{
    public async Task<bool> AddAsync(SysDictDataDto dto)
    {
        var entity = mapper.Map<SysDictDataEntity>(dto);
        return await sysDictDataRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await sysDictDataRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await sysDictDataRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(SysDictDataDto dto)
    {
        return await sysDictDataRepository.UpdateByDtoAsync(dto, mapper.Map<SysDictDataEntity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await sysDictDataRepository.UpdateAsync(new SysDictDataEntity { Id = id, State = state }, entity => entity.State, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<SysDictDataEntity>> PageAsync(SysDictDataPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<SysDictDataEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<SysDictDataEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await sysDictDataRepository.PageQueryAsync(WhereExpressionUtil.Create<SysDictDataEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(request.DictTypeId.HasValue, entity => entity.DictTypeId == request.DictTypeId)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.DictValue), entity => entity.DictValue.Contains(request.DictValue)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<List<SysDictDataEntity>> GetByTypeCodeAsync(string typeCode)
    {
        var dictType = await sysDictTypeRepository.GetAsync(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete && entity.Code == typeCode);
        if (dictType == null || dictType.Id < 1)
        {
            return new List<SysDictDataEntity>();
        }
        return (await sysDictDataRepository.QueryAsync(WhereExpressionUtil.Create<SysDictDataEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete && entity.DictTypeId == dictType.Id)))?.ToList() ?? new List<SysDictDataEntity>();
    }

    public async Task<SysDictDataEntity> GetByIdAsync(long id)
    {
        return await sysDictDataRepository.GetByIdAsync(id);
    }
}