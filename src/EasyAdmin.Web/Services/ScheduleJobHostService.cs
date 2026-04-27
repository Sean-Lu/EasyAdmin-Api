using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;

namespace EasyAdmin.Web.Services;

/// <summary>
/// 定时任务管理后台服务
/// </summary>
public class ScheduleJobHostService(
        ILogger<ScheduleJobHostService> logger,
        IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("定时任务管理服务启动中...");

        // 初始化加载任务
        await LoadEnabledJobsAsync();

        logger.LogInformation("定时任务管理服务已启动");
    }

    /// <summary>
    /// 加载并调度任务
    /// </summary>
    private async Task LoadEnabledJobsAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var scheduleJobService = scope.ServiceProvider.GetRequiredService<IScheduleJobService>();
        var jobs = await scheduleJobService.GetEnabledJobsAsync();
        
        var quartzSchedulerService = scope.ServiceProvider.GetRequiredService<QuartzSchedulerService>();
        await quartzSchedulerService.LoadEnabledJobsAsync(jobs);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("定时任务管理服务已停止");
        await base.StopAsync(cancellationToken);
    }
}
