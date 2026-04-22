using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;

namespace EasyAdmin.Application.Services;

public class SysDictTypeService(
    ILogger<SysDictTypeService> logger,
    IMapper mapper,
    ISysDictTypeRepository sysDictTypeRepository
    ) : ISysDictTypeService
{
    public async Task<bool> AddAsync(SysDictTypeDto dto)
    {
        var entity = mapper.Map<SysDictTypeEntity>(dto);
        return await sysDictTypeRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await sysDictTypeRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await sysDictTypeRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(SysDictTypeDto dto)
    {
        return await sysDictTypeRepository.UpdateByDtoAsync(dto, mapper.Map<SysDictTypeEntity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await sysDictTypeRepository.UpdateAsync(new SysDictTypeEntity { Id = id, State = state }, entity => entity.State, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<SysDictTypeEntity>> PageAsync(SysDictTypePageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<SysDictTypeEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<SysDictTypeEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await sysDictTypeRepository.PageQueryAsync(WhereExpressionUtil.Create<SysDictTypeEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Code), entity => entity.Code.Contains(request.Code)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<List<SysDictTypeEntity>> GetAllAsync()
    {
        return (await sysDictTypeRepository.QueryAsync(WhereExpressionUtil.Create<SysDictTypeEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)))?.ToList() ?? new List<SysDictTypeEntity>();
    }

    public async Task<SysDictTypeEntity> GetByIdAsync(long id)
    {
        return await sysDictTypeRepository.GetByIdAsync(id);
    }

    public async Task<SysDictTypeEntity> GetByCodeAsync(string code)
    {
        return await sysDictTypeRepository.GetAsync(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete && entity.Code == code);
    }
}