using EasyAdmin.ConsumerService.Contracts;
using Microsoft.Extensions.Configuration;
using Sean.Utility.Contracts;

namespace EasyAdmin.ConsumerService.Consumers;

public class TestConsumer : IBaseConsumer
{
    public bool IsStarted { get; private set; }

    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public TestConsumer(ISimpleLogger<TestConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public void Start()
    {
        if (IsStarted)
        {
            return;
        }

        //todo: 开始消费

        IsStarted = true;
    }

    public void Stop()
    {
        if (!IsStarted)
        {
            return;
        }

        //todo: 停止消费

        IsStarted = false;
    }
}