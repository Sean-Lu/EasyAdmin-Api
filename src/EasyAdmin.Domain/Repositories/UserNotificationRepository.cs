using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 用户通知仓储实现
/// </summary>
public class UserNotificationRepository(IConfiguration configuration, ILogger<UserNotificationRepository> logger)
    : BaseRepositoryExt<UserNotificationEntity>(configuration, logger), IUserNotificationRepository
{
}
