using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

public class ScheduleJobLogService(
    ILogger<ScheduleJobLogService> logger,
    IMapper mapper,
    IScheduleJobLogRepository scheduleJobLogRepository) : IScheduleJobLogService
{
    public async Task<bool> AddAsync(ScheduleJobLogDto dto)
    {
        var entity = mapper.Map<ScheduleJobLogEntity>(dto);
        return await scheduleJobLogRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await scheduleJobLogRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await scheduleJobLogRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> DeleteByJobIdAsync(long jobId)
    {
        await scheduleJobLogRepository.DeleteAsync(x => x.JobId == jobId);
        return true;
    }

    public async Task<PageQueryResult<ScheduleJobLogEntity>> PageAsync(ScheduleJobLogPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<ScheduleJobLogEntity>.Build(OrderByType.Desc, x => x.ExecuteStartTime);
        return await scheduleJobLogRepository.PageQueryAsync(
            WhereExpressionUtil.Create<ScheduleJobLogEntity>(x => !x.IsDelete && x.TenantId == TenantContextHolder.TenantId)
                .AndAlsoIF(request.JobId.HasValue, x => x.JobId == request.JobId.Value)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.JobName), x => x.JobName.Contains(request.JobName))
                .AndAlsoIF(request.ExecuteStatus.HasValue, x => x.ExecuteStatus == request.ExecuteStatus.Value),
            orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<ScheduleJobLogEntity> GetByIdAsync(long id)
    {
        return await scheduleJobLogRepository.GetByIdAsync(id);
    }
}