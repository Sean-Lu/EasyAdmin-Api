using System.Text.Json;
using EasyAdmin.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Application.Jobs;

public class TestJob(ILogger<TestJob> logger) : IDynamicJob
{
    public async Task ExecuteAsync(string? jobData)
    {
        TestJobData? data = null;
        if (!string.IsNullOrWhiteSpace(jobData))
        {
            try
            {
                data = JsonSerializer.Deserialize<TestJobData>(jobData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning("JobData 解析失败: {Message}", ex.Message);
            }
        }

        var message = data?.Message ?? "默认消息";
        var count = data?.Count ?? 1;

        logger.LogInformation("TestJob 开始执行: {Time}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        for (int i = 0; i < count; i++)
        {
            logger.LogInformation("TestJob 执行第 {Index} 次: {Message}", i + 1, message);
            await Task.Delay(100);
        }

        logger.LogInformation("TestJob 执行完成");
    }
}

public class TestJobData
{
    public string Message { get; set; } = "Hello";
    public int Count { get; set; } = 1;
}