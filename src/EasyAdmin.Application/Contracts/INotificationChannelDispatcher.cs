using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 通知通道分发器
/// </summary>
public interface INotificationChannelDispatcher
{
    /// <summary>
    /// 分发通知
    /// </summary>
    Task DispatchAsync(NotificationChannelDispatchRequest request, CancellationToken cancellationToken = default);
}
