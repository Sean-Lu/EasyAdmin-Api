using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 日志短信发送器
/// </summary>
public class LoggingSmsSender(
    ILogger<LoggingSmsSender> logger,
    IOptions<SmsOptions> options) : ISmsSender
{
    public Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        var smsOptions = options.Value;
        if (!smsOptions.Enable)
        {
            logger.LogInformation("短信发送未启用，跳过发送: Title={Title}", message.Title);
            return Task.CompletedTask;
        }

        var phoneNumbers = message.PhoneNumbers
            .Where(phoneNumber => !string.IsNullOrWhiteSpace(phoneNumber))
            .Select(phoneNumber => phoneNumber.Trim())
            .Distinct()
            .ToList();
        if (!phoneNumbers.Any())
        {
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "模拟短信发送: Title={Title}, PhoneCount={PhoneCount}, Phones={Phones}",
            message.Title,
            phoneNumbers.Count,
            string.Join(",", phoneNumbers));
        return Task.CompletedTask;
    }
}
