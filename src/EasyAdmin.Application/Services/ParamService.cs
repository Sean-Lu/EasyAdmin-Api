using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class ParamService(
    ILogger<ParamService> logger,
    IMapper mapper,
    IParamRepository paramRepository
    ) : IParamService
{
    public async Task<bool> AddAsync(ParamDto dto)
    {
        var entity = mapper.Map<ParamEntity>(dto);
        return await paramRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await paramRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await paramRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(ParamDto dto)
    {
        var entity = mapper.Map<ParamEntity>(dto);
        return await paramRepository.UpdateAsync(entity) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await paramRepository.UpdateAsync(new ParamEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<ParamEntity>> PageAsync(ParamPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<ParamEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<ParamEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await paramRepository.PageQueryAsync(WhereExpressionUtil.Create<ParamEntity>(entity => !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.ParamName), entity => entity.ParamName.Contains(request.ParamName))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.ParamKey), entity => entity.ParamKey.Contains(request.ParamKey))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.ParamValue), entity => entity.ParamValue.Contains(request.ParamValue)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<ParamEntity> GetByIdAsync(long id)
    {
        return await paramRepository.GetByIdAsync(id);
    }
}