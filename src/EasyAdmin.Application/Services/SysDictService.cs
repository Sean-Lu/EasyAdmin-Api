using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;

namespace EasyAdmin.Application.Services;

public class SysDictService(
    ILogger<SysDictService> logger,
    IMapper mapper,
    ISysDictRepository sysDictRepository
    ) : ISysDictService
{
    public async Task<bool> AddAsync(SysDictDto dto)
    {
        var entity = mapper.Map<SysDictEntity>(dto);
        return await sysDictRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await sysDictRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await sysDictRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(SysDictDto dto)
    {
        var entity = mapper.Map<SysDictEntity>(dto);
        return await sysDictRepository.UpdateAsync(entity) > 0;
    }

    public async Task<PageQueryResult<SysDictEntity>> PageAsync(SysDictPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<SysDictEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<SysDictEntity>.Build(OrderByType.Desc, entity => entity.Id);
        var pageQueryResult = await sysDictRepository.PageQueryAsync(WhereExpressionUtil.Create<SysDictEntity>(entity => !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Code), entity => entity.Code.Contains(request.Code))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.DictValue), entity => entity.DictValue.Contains(request.DictValue)), orderBy, request.PageNumber, request.PageSize);
        if (pageQueryResult.List != null && pageQueryResult.List.Any())
        {
            pageQueryResult.List = pageQueryResult.List.ToTreeList();
        }
        return pageQueryResult;
    }

    public async Task<SysDictEntity> GetByIdAsync(long id)
    {
        return await sysDictRepository.GetByIdAsync(id);
    }
}