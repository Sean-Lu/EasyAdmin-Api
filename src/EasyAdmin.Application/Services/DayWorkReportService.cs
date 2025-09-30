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

public class DayWorkReportService(
    ILogger<DayWorkReportService> logger,
    IMapper mapper,
    IDayWorkReportRepository dayWorkReportRepository
    ) : IDayWorkReportService
{
    public async Task<bool> AddAsync(DayWorkReportDto dto)
    {
        var entity = mapper.Map<DayWorkReportEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await dayWorkReportRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await dayWorkReportRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await dayWorkReportRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(DayWorkReportDto dto)
    {
        var entity = mapper.Map<DayWorkReportEntity>(dto);
        return await dayWorkReportRepository.UpdateAsync(entity) > 0;
    }

    public async Task<PageQueryResult<DayWorkReportEntity>> PageAsync(DayWorkReportPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<DayWorkReportEntity>.Build(OrderByType.Desc, entity => entity.RecordTime);
        orderBy.Next = OrderByConditionBuilder<DayWorkReportEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await dayWorkReportRepository.PageQueryAsync(WhereExpressionUtil.Create<DayWorkReportEntity>(entity => entity.UserId == TenantContextHolder.UserId && !entity.IsDelete)
            .AndAlsoIF(request.StartTime.HasValue, entity => entity.RecordTime >= request.StartTime)
            .AndAlsoIF(request.EndTime.HasValue, entity => entity.RecordTime <= request.EndTime), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<DayWorkReportEntity> GetByIdAsync(long id)
    {
        return await dayWorkReportRepository.GetByIdAsync(id);
    }
}