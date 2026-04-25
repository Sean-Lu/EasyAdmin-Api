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

public class MonthWorkReportService(
    ILogger<MonthWorkReportService> logger,
    IMapper mapper,
    IMonthWorkReportRepository monthWorkReportRepository
    ) : IMonthWorkReportService
{
    public async Task<bool> AddAsync(MonthWorkReportDto dto)
    {
        var entity = mapper.Map<MonthWorkReportEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await monthWorkReportRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await monthWorkReportRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await monthWorkReportRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(MonthWorkReportDto dto)
    {
        return await monthWorkReportRepository.UpdateByDtoAsync(dto, mapper.Map<MonthWorkReportEntity>) > 0;
    }

    public async Task<PageQueryResult<MonthWorkReportEntity>> PageAsync(MonthWorkReportPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<MonthWorkReportEntity>.Build(OrderByType.Desc, entity => entity.StartTime);
        orderBy.Next = OrderByConditionBuilder<MonthWorkReportEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await monthWorkReportRepository.PageQueryAsync(WhereExpressionUtil.Create<MonthWorkReportEntity>(entity => entity.UserId == TenantContextHolder.UserId && !entity.IsDelete)
            .AndAlsoIF(request.StartTime.HasValue, entity => entity.StartTime >= request.StartTime)
            .AndAlsoIF(request.EndTime.HasValue, entity => entity.EndTime <= request.EndTime), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<MonthWorkReportEntity> GetByIdAsync(long id)
    {
        return await monthWorkReportRepository.GetByIdAsync(id);
    }
}
