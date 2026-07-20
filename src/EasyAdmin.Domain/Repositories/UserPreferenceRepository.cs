using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 用户偏好仓储实现
/// </summary>
public class UserPreferenceRepository(
    IConfiguration configuration,
    ILogger<UserPreferenceRepository> logger)
    : BaseRepositoryExt<UserPreferenceEntity>(configuration, logger), IUserPreferenceRepository
{
}
