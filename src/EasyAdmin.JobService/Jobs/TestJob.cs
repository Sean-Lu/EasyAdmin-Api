using Quartz;
using Sean.Utility.Contracts;
using Sean.Utility.Extensions;

namespace EasyAdmin.JobService.Jobs;

/// <summary>
/// 测试Job
/// </summary>
[DisallowConcurrentExecution]
public class TestJob(ISimpleLogger<TestJob> logger) : IJob
{
    private readonly ILogger _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        var jobName = context.JobDetail.Key.Name;
        _logger.LogInfo($"【{jobName}】{DateTime.Now.ToLongDateTime()}");
        return Task.CompletedTask;
    }
}