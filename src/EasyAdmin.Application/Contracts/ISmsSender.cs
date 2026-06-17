using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 短信发送器
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// 发送短信
    /// </summary>
    Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
}
