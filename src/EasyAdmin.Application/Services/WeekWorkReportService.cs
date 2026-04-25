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

public class WeekWorkReportService(
    ILogger<WeekWorkReportService> logger,
    IMapper mapper,
    IWeekWorkReportRepository weekWorkReportRepository
    ) : IWeekWorkReportService
{
    public async Task<bool> AddAsync(WeekWorkReportDto dto)
    {
        var entity = mapper.Map<WeekWorkReportEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await weekWorkReportRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await weekWorkReportRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await weekWorkReportRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(WeekWorkReportDto dto)
    {
        return await weekWorkReportRepository.UpdateByDtoAsync(dto, mapper.Map<WeekWorkReportEntity>) > 0;
    }

    public async Task<PageQueryResult<WeekWorkReportEntity>> PageAsync(WeekWorkReportPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<WeekWorkReportEntity>.Build(OrderByType.Desc, entity => entity.StartTime);
        orderBy.Next = OrderByConditionBuilder<WeekWorkReportEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await weekWorkReportRepository.PageQueryAsync(WhereExpressionUtil.Create<WeekWorkReportEntity>(entity => entity.UserId == TenantContextHolder.UserId && !entity.IsDelete)
            .AndAlsoIF(request.StartTime.HasValue, entity => entity.StartTime >= request.StartTime)
            .AndAlsoIF(request.EndTime.HasValue, entity => entity.EndTime <= request.EndTime), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<WeekWorkReportEntity> GetByIdAsync(long id)
    {
        return await weekWorkReportRepository.GetByIdAsync(id);
    }
}
