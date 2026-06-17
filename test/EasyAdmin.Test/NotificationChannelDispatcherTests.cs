using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyAdmin.Test;

[TestClass]
public class NotificationChannelDispatcherTests
{
    [TestMethod]
    public async Task DispatchAsync_SendsEmailOnlyToRecipientsWithEmail()
    {
        var emailSender = new FakeEmailSender();
        var smsSender = new FakeSmsSender();
        var dispatcher = CreateDispatcher(emailSender, smsSender);

        await dispatcher.DispatchAsync(new NotificationChannelDispatchRequest
        {
            Title = "System notice",
            Content = "Email body",
            SendEmail = true,
            Recipients = new List<UserEntity>
            {
                new() { Id = 1, Email = "one@example.com", PhoneNumber = "13800000001" },
                new() { Id = 2, Email = "", PhoneNumber = "13800000002" },
                new() { Id = 3, Email = "   ", PhoneNumber = "13800000003" }
            }
        });

        Assert.AreEqual(1, emailSender.Messages.Count);
        CollectionAssert.AreEqual(new[] { "one@example.com" }, emailSender.Messages[0].ToAddresses.ToArray());
        Assert.AreEqual(0, smsSender.Messages.Count);
    }

    [TestMethod]
    public async Task DispatchAsync_SendsSmsOnlyToRecipientsWithPhoneNumber()
    {
        var emailSender = new FakeEmailSender();
        var smsSender = new FakeSmsSender();
        var dispatcher = CreateDispatcher(emailSender, smsSender);

        await dispatcher.DispatchAsync(new NotificationChannelDispatchRequest
        {
            Title = "System notice",
            Content = "SMS body",
            SendSms = true,
            Recipients = new List<UserEntity>
            {
                new() { Id = 1, Email = "one@example.com", PhoneNumber = "13800000001" },
                new() { Id = 2, Email = "two@example.com", PhoneNumber = "" },
                new() { Id = 3, Email = "three@example.com", PhoneNumber = "   " }
            }
        });

        Assert.AreEqual(0, emailSender.Messages.Count);
        Assert.AreEqual(1, smsSender.Messages.Count);
        CollectionAssert.AreEqual(new[] { "13800000001" }, smsSender.Messages[0].PhoneNumbers.ToArray());
    }

    [TestMethod]
    public async Task DispatchAsync_DoesNotCallSendersWhenExternalChannelsDisabled()
    {
        var emailSender = new FakeEmailSender();
        var smsSender = new FakeSmsSender();
        var dispatcher = CreateDispatcher(emailSender, smsSender);

        await dispatcher.DispatchAsync(new NotificationChannelDispatchRequest
        {
            Title = "System notice",
            Content = "Body",
            SendEmail = false,
            SendSms = false,
            Recipients = new List<UserEntity>
            {
                new() { Id = 1, Email = "one@example.com", PhoneNumber = "13800000001" }
            }
        });

        Assert.AreEqual(0, emailSender.Messages.Count);
        Assert.AreEqual(0, smsSender.Messages.Count);
    }

    [TestMethod]
    public async Task DispatchAsync_SwallowsSenderExceptions()
    {
        var emailSender = new FakeEmailSender { ThrowOnSend = true };
        var smsSender = new FakeSmsSender { ThrowOnSend = true };
        var dispatcher = CreateDispatcher(emailSender, smsSender);

        await dispatcher.DispatchAsync(new NotificationChannelDispatchRequest
        {
            Title = "System notice",
            Content = "Body",
            SendEmail = true,
            SendSms = true,
            Recipients = new List<UserEntity>
            {
                new() { Id = 1, Email = "one@example.com", PhoneNumber = "13800000001" }
            }
        });

        Assert.AreEqual(1, emailSender.Attempts);
        Assert.AreEqual(1, smsSender.Attempts);
    }

    private static NotificationChannelDispatcher CreateDispatcher(IEmailSender emailSender, ISmsSender smsSender)
    {
        return new NotificationChannelDispatcher(
            NullLogger<NotificationChannelDispatcher>.Instance,
            emailSender,
            smsSender);
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public List<EmailMessage> Messages { get; } = new();
        public int Attempts { get; private set; }
        public bool ThrowOnSend { get; init; }

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Attempts++;
            if (ThrowOnSend)
            {
                throw new InvalidOperationException("email failed");
            }

            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSmsSender : ISmsSender
    {
        public List<SmsMessage> Messages { get; } = new();
        public int Attempts { get; private set; }
        public bool ThrowOnSend { get; init; }

        public Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
        {
            Attempts++;
            if (ThrowOnSend)
            {
                throw new InvalidOperationException("sms failed");
            }

            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
