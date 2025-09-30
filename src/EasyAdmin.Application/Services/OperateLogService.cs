using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;

namespace EasyAdmin.Application.Services;

public class OperateLogService(
    ILogger<OperateLogService> logger,
    IMapper mapper,
    IOperateLogRepository operateLogRepository
    ) : IOperateLogService
{
    public async Task<bool> AddAsync(OperateLogDto dto)
    {
        var entity = mapper.Map<OperateLogEntity>(dto);
        return await operateLogRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await operateLogRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await operateLogRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(OperateLogDto dto)
    {
        var entity = mapper.Map<OperateLogEntity>(dto);
        return await operateLogRepository.UpdateAsync(entity) > 0;
    }

    public async Task<PageQueryResult<OperateLogEntity>> PageAsync(OperateLogPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<OperateLogEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<OperateLogEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await operateLogRepository.PageQueryAsync(WhereExpressionUtil.Create<OperateLogEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(request.UserId.HasValue && request.UserId.Value > 0, entity => entity.UserId == request.UserId.GetValueOrDefault())
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.IP), entity => entity.IP.Contains(request.IP)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<OperateLogEntity> GetByIdAsync(long id)
    {
        return await operateLogRepository.GetByIdAsync(id);
    }
}