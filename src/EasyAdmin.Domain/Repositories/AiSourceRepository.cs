using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// AI引用来源仓储实现
/// </summary>
public class AiSourceRepository(IConfiguration configuration, ILogger<AiSourceRepository> logger) : BaseRepositoryExt<AiSourceEntity>(configuration, logger), IAiSourceRepository
{
}
