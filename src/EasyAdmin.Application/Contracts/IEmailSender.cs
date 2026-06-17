using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 邮件发送器
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
