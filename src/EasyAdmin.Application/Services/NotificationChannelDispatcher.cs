using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 通知通道分发器
/// </summary>
public class NotificationChannelDispatcher(
    ILogger<NotificationChannelDispatcher> logger,
    IEmailSender emailSender,
    ISmsSender smsSender) : INotificationChannelDispatcher
{
    public async Task DispatchAsync(NotificationChannelDispatchRequest request, CancellationToken cancellationToken = default)
    {
        if (request.SendEmail)
        {
            var emailAddresses = request.Recipients
                .Select(user => user.Email)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (emailAddresses.Any())
            {
                try
                {
                    await emailSender.SendAsync(new EmailMessage
                    {
                        Subject = request.Title,
                        Body = request.Content,
                        ToAddresses = emailAddresses
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "邮件通知发送失败: Title={Title}", request.Title);
                }
            }
        }

        if (request.SendSms)
        {
            var phoneNumbers = request.Recipients
                .Select(user => user.PhoneNumber)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct()
                .ToList();

            if (phoneNumbers.Any())
            {
                try
                {
                    await smsSender.SendAsync(new SmsMessage
                    {
                        Title = request.Title,
                        Content = request.Content,
                        PhoneNumbers = phoneNumbers
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "短信通知发送失败: Title={Title}", request.Title);
                }
            }
        }
    }
}
