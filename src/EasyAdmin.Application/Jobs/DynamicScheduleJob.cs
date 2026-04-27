using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace EasyAdmin.Application.Jobs;

/// <summary>
/// 动态执行任务的Job
/// </summary>
[DisallowConcurrentExecution]
public class DynamicScheduleJob : IJob
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public DynamicScheduleJob(ILogger<DynamicScheduleJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var startTime = DateTime.Now;
        var jobId = context.JobDetail.JobDataMap.GetLong("JobId");
        var jobName = context.JobDetail.JobDataMap.GetString("JobName") ?? "";
        var jobGroup = context.JobDetail.JobDataMap.GetString("JobGroup") ?? "";
        var jobClassName = context.JobDetail.JobDataMap.GetString("JobClassName") ?? "";
        var jobData = context.JobDetail.JobDataMap.GetString("JobData") ?? "";
        var tenantId = context.JobDetail.JobDataMap.GetLong("TenantId");

        // 设置租户上下文
        if (tenantId > 0)
        {
            TenantContextHolder.UserInfo = new JwtUserModel { TenantId = tenantId };
        }

        ScheduleJobLogDto? logDto = null;

        using var scope = _serviceProvider.CreateScope();
        var logService = scope.ServiceProvider.GetRequiredService<IScheduleJobLogService>();
        var jobService = scope.ServiceProvider.GetRequiredService<IScheduleJobService>();

        try
        {
            _logger.LogInformation($"开始执行任务: {jobGroup} - {jobName}, 租户ID: {tenantId}");

            // 创建执行日志
            logDto = new ScheduleJobLogDto
            {
                JobId = jobId,
                JobName = jobName,
                JobGroup = jobGroup,
                ExecuteStartTime = startTime,
                ExecuteStatus = 1
            };

            // 尝试反射执行任务
            var executeMessage = await ExecuteJobByReflectionAsync(jobClassName, jobData, scope.ServiceProvider);
            logDto.ExecuteMessage = executeMessage;

            var endTime = DateTime.Now;
            logDto.ExecuteEndTime = endTime;
            logDto.ExecuteElapsedTime = (long)(endTime - startTime).TotalMilliseconds;

            // 更新任务的最后执行时间
            await jobService.UpdateExecuteTimeAsync(jobId, startTime, null);

            _logger.LogInformation($"任务执行完成: {jobGroup} - {jobName}, 耗时: {logDto.ExecuteElapsedTime}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError($"任务执行失败: {jobGroup} - {jobName}, 错误: {ex.Message}", ex);

            if (logDto != null)
            {
                logDto.ExecuteStatus = 0;
                logDto.ExecuteMessage = $"执行异常: {ex.Message}";
                logDto.ExecuteEndTime = DateTime.Now;
                logDto.ExecuteElapsedTime = (long)(logDto.ExecuteEndTime.Value - startTime).TotalMilliseconds;
            }
        }
        finally
        {
            try
            {
                // 保存执行日志
                if (logDto != null)
                {
                    try
                    {
                        await logService.AddAsync(logDto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"保存执行日志失败: {ex.Message}", ex);
                    }
                }
            }
            finally
            {
                // 清理租户上下文
                TenantContextHolder.Clear();
            }
        }
    }

    /// <summary>
    /// 通过反射执行任务
    /// </summary>
    private async Task<string> ExecuteJobByReflectionAsync(string jobClassName, string jobData, IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(jobClassName))
        {
            return "未配置任务类名，默认执行成功";
        }

        try
        {
            // 尝试加载类型
            var jobType = Type.GetType(jobClassName);
            if (jobType == null)
            {
                // 尝试在当前加载的程序集中查找
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    jobType = assembly.GetType(jobClassName);
                    if (jobType != null) break;
                }
            }

            if (jobType == null)
            {
                return $"找不到任务类型: {jobClassName}";
            }

            // 创建实例
            var instance = ActivatorUtilities.CreateInstance(serviceProvider, jobType);

            if (instance is IDynamicJob dynamicJob)
            {
                await dynamicJob.ExecuteAsync(jobData);
                return "执行成功";
            }

            // 查找Execute方法
            var executeMethod = jobType.GetMethod("Execute");
            if (executeMethod == null)
            {
                return $"任务类型 {jobClassName} 没有找到Execute方法，且未实现IDynamicJob接口";
            }

            // 执行方法
            var parameters = executeMethod.GetParameters();
            object?[] args = parameters.Length > 0 ? new object?[parameters.Length] : Array.Empty<object?>();

            // 尝试执行
            var result = executeMethod.Invoke(instance, args);
            if (result is Task task)
            {
                await task;
            }

            return "执行成功";
        }
        catch (Exception ex)
        {
            return $"执行任务失败: {ex.Message}";
        }
    }
}
