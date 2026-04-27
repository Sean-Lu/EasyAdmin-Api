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

public class ScheduleJobService(
    ILogger<ScheduleJobService> logger,
    IMapper mapper,
    IScheduleJobRepository scheduleJobRepository,
    QuartzSchedulerService quartzSchedulerService) : IScheduleJobService
{
    public async Task<bool> AddAsync(ScheduleJobDto dto)
    {
        var entity = mapper.Map<ScheduleJobEntity>(dto);
        var result = await scheduleJobRepository.AddAsync(entity);
        if (result && entity.Id > 0)
        {
            // 同步到 Quartz
            await SyncToQuartzAsync(entity.Id);
        }
        return result;
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        var result = await scheduleJobRepository.DeleteByIdAsync(id);
        if (result)
        {
            // 从 Quartz 中删除
            await quartzSchedulerService.StopJobAsync(id);
        }
        return result;
    }

    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        var result = await scheduleJobRepository.DeleteByIdsAsync(ids);
        if (result && ids.Any())
        {
            // 从 Quartz 中删除
            foreach (var jobId in ids)
            {
                await quartzSchedulerService.StopJobAsync(jobId);
            }
        }
        return result;
    }

    public async Task<bool> UpdateAsync(ScheduleJobUpdateDto dto)
    {
        var result = await scheduleJobRepository.UpdateByDtoAsync(dto, mapper.Map<ScheduleJobEntity>) > 0;
        if (result && dto.Id > 0)
        {
            // 同步到 Quartz
            await SyncToQuartzAsync(dto.Id);
        }
        return result;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        var result = await scheduleJobRepository.UpdateAsync(
            new ScheduleJobEntity
            {
                State = state
            },
            x => new { x.State },
            x => x.Id == id) > 0;
        if (result)
        {
            // 同步到 Quartz
            await SyncToQuartzAsync(id);
        }
        return result;
    }

    public async Task<PageQueryResult<ScheduleJobEntity>> PageAsync(ScheduleJobPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<ScheduleJobEntity>.Build(OrderByType.Desc, x => x.CreateTime);
        orderBy.Next = OrderByConditionBuilder<ScheduleJobEntity>.Build(OrderByType.Desc, x => x.Id);
        return await scheduleJobRepository.PageQueryAsync(
            WhereExpressionUtil.Create<ScheduleJobEntity>(x => !x.IsDelete && x.TenantId == TenantContextHolder.TenantId)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.JobName), x => x.JobName.Contains(request.JobName))
                .AndAlsoIF(request.State.HasValue, x => x.State == (CommonState)request.State.Value),
            orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<ScheduleJobEntity> GetByIdAsync(long id)
    {
        return await scheduleJobRepository.GetByIdAsync(id);
    }

    public async Task<bool> RunOnceAsync(long id)
    {
        try
        {
            logger.LogInformation($"请求立即执行任务: {id}");
            return await quartzSchedulerService.RunJobNowAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError($"立即执行任务失败: {id}, 错误: {ex.Message}", ex);
            return false;
        }
    }

    public async Task<List<ScheduleJobEntity>> GetEnabledJobsAsync()
    {
        return (await scheduleJobRepository.QueryAsync(x => x.State == CommonState.Enable && !x.IsDelete)).ToList();
    }

    public async Task<bool> UpdateExecuteTimeAsync(long id, DateTime? lastExecuteTime, DateTime? nextExecuteTime)
    {
        return await scheduleJobRepository.UpdateAsync(
            new ScheduleJobEntity
            {
                LastExecuteTime = lastExecuteTime,
                NextExecuteTime = nextExecuteTime
            },
            x => new { x.LastExecuteTime, x.NextExecuteTime },
            x => x.Id == id) > 0;
    }

    public async Task<bool> SyncToQuartzAsync(long jobId)
    {
        var job = await scheduleJobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            logger.LogError($"任务不存在: {jobId}");
            return false;
        }
        return await quartzSchedulerService.AddOrUpdateJobAsync(job);
    }

    public async Task<bool> SyncToQuartzAsync(List<long> jobIds)
    {
        bool allSuccess = true;
        foreach (var jobId in jobIds)
        {
            var success = await SyncToQuartzAsync(jobId);
            if (!success)
            {
                allSuccess = false;
            }
        }
        return allSuccess;
    }
}
