using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class NotificationRepository(IConfiguration configuration, ILogger<NotificationRepository> logger)
    : BaseRepositoryExt<NotificationEntity>(configuration, logger), INotificationRepository
{
}
