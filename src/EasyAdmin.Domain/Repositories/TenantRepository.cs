using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class TenantRepository(IConfiguration configuration, ILogger<TenantRepository> logger) : BaseRepositoryExt<TenantEntity>(configuration, logger), ITenantRepository
{

}