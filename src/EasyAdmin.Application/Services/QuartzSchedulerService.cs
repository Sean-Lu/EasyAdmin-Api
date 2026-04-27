using EasyAdmin.Application.Jobs;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EasyAdmin.Application.Services;

/// <summary>
/// Quartz 调度服务
/// </summary>
public class QuartzSchedulerService(
    ILogger<QuartzSchedulerService> logger,
    ISchedulerFactory schedulerFactory)
{
    /// <summary>
    /// 默认任务组常量
    /// </summary>
    private const string DefaultJobGroup = "DEFAULT";

    /// <summary>
    /// 获取调度器实例
    /// </summary>
    private async Task<IScheduler> GetSchedulerAsync()
    {
        return await schedulerFactory.GetScheduler();
    }

    /// <summary>
    /// 调度任务
    /// </summary>
    public async Task<bool> ScheduleJobAsync(ScheduleJobEntity job)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(job.Id.ToString(), DefaultJobGroup);
            var triggerKey = new TriggerKey(job.Id.ToString(), DefaultJobGroup);

            // 构建JobDetail
            var jobDetail = JobBuilder.Create<DynamicScheduleJob>()
                .WithIdentity(jobKey)
                .UsingJobData("JobId", job.Id)
                .UsingJobData("JobName", job.JobName)
                .UsingJobData("JobGroup", DefaultJobGroup)
                .UsingJobData("JobClassName", job.JobClassName)
                .UsingJobData("JobData", job.JobData ?? "")
                .UsingJobData("TenantId", job.TenantId)
                .Build();

            // 构建Trigger
            ITrigger trigger;
            if (job.ScheduleType == ScheduleType.Cron && !string.IsNullOrWhiteSpace(job.CronExpression))
            {
                // Cron调度
                trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithCronSchedule(job.CronExpression)
                    .Build();
            }
            else
            {
                // 简单调度
                int interval = job.SimpleInterval ?? 1;
                TimeSpan timeSpan = job.SimpleIntervalUnit switch
                {
                    SimpleIntervalUnit.Second => TimeSpan.FromSeconds(interval),
                    SimpleIntervalUnit.Hour => TimeSpan.FromHours(interval),
                    SimpleIntervalUnit.Day => TimeSpan.FromDays(interval),
                    _ => TimeSpan.FromMinutes(interval)
                };

                trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey)
                    .WithSimpleSchedule(x => x.WithInterval(timeSpan).RepeatForever())
                    .Build();
            }

            // 调度任务
            await scheduler.ScheduleJob(jobDetail, trigger);
            logger.LogInformation($"加载任务成功: {job.JobName}");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"加载任务失败: {job.JobName}, 错误: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// 停止任务
    /// </summary>
    public async Task<bool> StopJobAsync(ScheduleJobEntity job)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(job.Id.ToString(), DefaultJobGroup);
            if (await scheduler.CheckExists(jobKey))
            {
                await scheduler.DeleteJob(jobKey);
                logger.LogInformation($"停止任务: {job.JobName}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError($"停止任务失败: {job.JobName}, 错误: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// 停止任务（通过ID）
    /// </summary>
    public async Task<bool> StopJobAsync(long jobId)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(jobId.ToString(), DefaultJobGroup);
            if (await scheduler.CheckExists(jobKey))
            {
                await scheduler.DeleteJob(jobKey);
                logger.LogInformation($"停止任务: {jobId}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError($"停止任务失败: {jobId}, 错误: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// 立即执行任务
    /// </summary>
    public async Task<bool> RunJobNowAsync(long jobId)
    {
        try
        {
            var scheduler = await GetSchedulerAsync();
            var jobKey = new JobKey(jobId.ToString(), DefaultJobGroup);
            if (await scheduler.CheckExists(jobKey))
            {
                await scheduler.TriggerJob(jobKey);
                logger.LogInformation($"手动触发任务: {jobId}");
                return true;
            }
            else
            {
                logger.LogError($"任务不存在: {jobId}");
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"手动触发任务失败: {jobId}, 错误: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// 新增或更新任务
    /// </summary>
    public async Task<bool> AddOrUpdateJobAsync(ScheduleJobEntity job)
    {
        if (job.State == CommonState.Enable)
        {
            // 启用状态，调度任务
            await StopJobAsync(job);
            return await ScheduleJobAsync(job);
        }
        else
        {
            // 禁用状态，停止任务
            return await StopJobAsync(job);
        }
    }

    /// <summary>
    /// 批量加载启用的任务
    /// </summary>
    public async Task LoadEnabledJobsAsync(List<ScheduleJobEntity> jobs)
    {
        if (jobs == null || !jobs.Any())
        {
            logger.LogInformation("当前没有启用的定时任务");
            return;
        }

        logger.LogInformation($"开始加载 {jobs.Count} 个定时任务...");

        foreach (var job in jobs)
        {
            await ScheduleJobAsync(job);
        }

        logger.LogInformation("定时任务加载完成");
    }
}
