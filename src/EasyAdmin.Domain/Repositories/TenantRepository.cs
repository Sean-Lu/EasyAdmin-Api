using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 租户仓储实现
/// </summary>
public class TenantRepository(IConfiguration configuration, ILogger<TenantRepository> logger) : BaseRepositoryExt<TenantEntity>(configuration, logger), ITenantRepository
{

}