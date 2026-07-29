using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// AI租户设置仓储实现
/// </summary>
public class AiTenantSettingRepository(IConfiguration configuration, ILogger<AiTenantSettingRepository> logger) : BaseRepositoryExt<AiTenantSettingEntity>(configuration, logger), IAiTenantSettingRepository
{
}
