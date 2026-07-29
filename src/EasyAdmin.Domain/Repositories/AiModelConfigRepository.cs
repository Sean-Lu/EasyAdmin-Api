using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// AI模型配置仓储实现
/// </summary>
public class AiModelConfigRepository(IConfiguration configuration, ILogger<AiModelConfigRepository> logger) : BaseRepositoryExt<AiModelConfigEntity>(configuration, logger), IAiModelConfigRepository
{
}
