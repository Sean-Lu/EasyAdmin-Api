using EasyAdmin.JobService.Jobs;
using Microsoft.Extensions.Hosting;
using Sean.Core.Quartz;
using Sean.Utility;
using Sean.Utility.Contracts;
using Sean.Utility.Extensions;
using Sean.Utility.Impls.Log;

namespace EasyAdmin.JobService;

public class MainService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly JobManager _jobManager;

    public MainService(ISimpleLogger<MainService> logger)
    {
        _logger = logger;
        _jobManager = new JobManager(options =>
        {

        });

        SimpleLocalLoggerBase.DateTimeFormat = time => time.ToLongDateTime();

        ExceptionHelper.CatchGlobalUnhandledException(_logger);

        var list = new List<JobOptions>
        {
            new()
            {
                JobType = typeof(TestJob),
                ScheduleType = ScheduleType.SimpleSchedule,
                SimpleScheduleAction = c => c.WithIntervalInSeconds(1).RepeatForever(), // 每秒执行1次
                IsStartNow = true
            },
        };

        list.ForEach(c => _jobManager.ScheduleJob(c));
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _jobManager.Start(null, cancellationToken);
        _logger.LogInfo($"服务[{typeof(MainService).Namespace}]开始运行");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _jobManager.Stop(true, cancellationToken);
        _logger.LogInfo($"服务[{typeof(MainService).Namespace}]停止运行");
        await base.StopAsync(cancellationToken);
    }
}