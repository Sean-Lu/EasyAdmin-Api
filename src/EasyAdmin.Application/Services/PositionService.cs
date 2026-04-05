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

public class PositionService(
    ILogger<PositionService> logger,
    IMapper mapper,
    IPositionRepository positionRepository
    ) : IPositionService
{
    public async Task<bool> AddAsync(PositionDto dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
        {
            throw new ExplicitException("岗位名称不能为空");
        }
        if (string.IsNullOrEmpty(dto.Code))
        {
            throw new ExplicitException("岗位编码不能为空");
        }

        var entity = mapper.Map<PositionEntity>(dto);
        return await positionRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await positionRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await positionRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(PositionDto dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
        {
            throw new ExplicitException("岗位名称不能为空");
        }
        if (string.IsNullOrEmpty(dto.Code))
        {
            throw new ExplicitException("岗位编码不能为空");
        }

        return await positionRepository.UpdateByDtoAsync(dto, mapper.Map<PositionEntity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await positionRepository.UpdateAsync(new PositionEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<PositionEntity>> PageAsync(PositionPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<PositionEntity>.Build(OrderByType.Asc, entity => entity.Sort);
        orderBy.Next = OrderByConditionBuilder<PositionEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await positionRepository.PageQueryAsync(WhereExpressionUtil.Create<PositionEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Name), entity => entity.Name.Contains(request.Name))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Code), entity => entity.Code.Contains(request.Code)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<List<PositionEntity>> GetListAsync()
    {
        return (await positionRepository.QueryAsync(WhereExpressionUtil.Create<PositionEntity>(entity => !entity.IsDelete)
            .AndAlso(entity => entity.State == CommonState.Enable)))?.ToList() ?? new List<PositionEntity>();
    }

    public async Task<PositionEntity> GetByIdAsync(long id)
    {
        return await positionRepository.GetByIdAsync(id);
    }
}
