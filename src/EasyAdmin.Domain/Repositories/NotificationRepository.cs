using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 通知仓储实现
/// </summary>
public class NotificationRepository(IConfiguration configuration, ILogger<NotificationRepository> logger)
    : BaseRepositoryExt<NotificationEntity>(configuration, logger), INotificationRepository
{
}
