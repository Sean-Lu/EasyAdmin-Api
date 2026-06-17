using System.Net;
using System.Net.Mail;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyAdmin.Application.Services;

/// <summary>
/// SMTP 邮件发送器
/// </summary>
public class SmtpEmailSender(
    ILogger<SmtpEmailSender> logger,
    IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var emailOptions = options.Value;
        if (!emailOptions.Enable)
        {
            logger.LogInformation("邮件发送未启用，跳过发送: Subject={Subject}", message.Subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(emailOptions.Host) || string.IsNullOrWhiteSpace(emailOptions.FromAddress))
        {
            logger.LogWarning("邮件发送配置不完整，跳过发送: Subject={Subject}", message.Subject);
            return;
        }

        var toAddresses = message.ToAddresses
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Select(address => address.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (!toAddresses.Any())
        {
            return;
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(emailOptions.FromAddress, emailOptions.FromName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = false
        };

        foreach (var toAddress in toAddresses)
        {
            mailMessage.To.Add(toAddress);
        }

        using var smtpClient = new SmtpClient(emailOptions.Host, emailOptions.Port)
        {
            EnableSsl = emailOptions.UseSsl
        };

        if (!string.IsNullOrWhiteSpace(emailOptions.UserName))
        {
            smtpClient.Credentials = new NetworkCredential(emailOptions.UserName, emailOptions.Password);
        }

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}
