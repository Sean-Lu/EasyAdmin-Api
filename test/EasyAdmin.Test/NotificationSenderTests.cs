using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EasyAdmin.Test;

[TestClass]
public class NotificationSenderTests
{
    [TestMethod]
    public async Task SmtpEmailSender_DoesNotThrowWhenDisabled()
    {
        var sender = new SmtpEmailSender(
            NullLogger<SmtpEmailSender>.Instance,
            Options.Create(new EmailOptions { Enable = false }));

        await sender.SendAsync(new EmailMessage
        {
            Subject = "Disabled",
            Body = "Body",
            ToAddresses = new List<string> { "one@example.com" }
        });
    }

    [TestMethod]
    public async Task SmtpEmailSender_DoesNotThrowWhenEnabledButConfigIncomplete()
    {
        var sender = new SmtpEmailSender(
            NullLogger<SmtpEmailSender>.Instance,
            Options.Create(new EmailOptions { Enable = true }));

        await sender.SendAsync(new EmailMessage
        {
            Subject = "Incomplete",
            Body = "Body",
            ToAddresses = new List<string> { "one@example.com" }
        });
    }

    [TestMethod]
    public async Task LoggingSmsSender_DoesNotThrowWhenEnabled()
    {
        var sender = new LoggingSmsSender(
            NullLogger<LoggingSmsSender>.Instance,
            Options.Create(new SmsOptions { Enable = true }));

        await sender.SendAsync(new SmsMessage
        {
            Title = "SMS",
            Content = "Body",
            PhoneNumbers = new List<string> { "13800000001" }
        });
    }

    [TestMethod]
    public async Task LoggingSmsSender_DoesNotThrowWhenDisabled()
    {
        var sender = new LoggingSmsSender(
            NullLogger<LoggingSmsSender>.Instance,
            Options.Create(new SmsOptions { Enable = false }));

        await sender.SendAsync(new SmsMessage
        {
            Title = "SMS",
            Content = "Body",
            PhoneNumbers = new List<string> { "13800000001" }
        });
    }
}
