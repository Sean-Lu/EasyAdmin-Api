using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// AI调用用量仓储实现
/// </summary>
public class AiUsageRepository(IConfiguration configuration, ILogger<AiUsageRepository> logger) : BaseRepositoryExt<AiUsageEntity>(configuration, logger), IAiUsageRepository
{
}
