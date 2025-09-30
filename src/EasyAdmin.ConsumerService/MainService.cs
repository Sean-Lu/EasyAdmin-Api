using EasyAdmin.ConsumerService.Consumers;
using EasyAdmin.ConsumerService.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sean.Utility;
using Sean.Utility.Contracts;
using Sean.Utility.Extensions;
using Sean.Utility.Impls.Log;

namespace EasyAdmin.ConsumerService;

public class MainService : IHostedService
{
    private readonly ILogger _logger;
    private readonly List<IBaseConsumer> _services;

    public MainService(ISimpleLogger<MainService> logger, IServiceProvider provider)
    {
        _logger = logger;
        _services = new List<IBaseConsumer>();

        SimpleLocalLoggerBase.DateTimeFormat = time => time.ToLongDateTime();

        ExceptionHelper.CatchGlobalUnhandledException(_logger);

        _services.Add(provider.GetService<TestConsumer>());
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _services.ForEach(service => service.Start());
        _logger.LogInfo($"[{typeof(MainService).Namespace}]服务开始运行");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _services.ForEach(service => service.Stop());
        _logger.LogInfo($"[{typeof(MainService).Namespace}]服务停止运行");
        return Task.CompletedTask;
    }
}