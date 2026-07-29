using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// AI消息仓储实现
/// </summary>
public class AiMessageRepository(IConfiguration configuration, ILogger<AiMessageRepository> logger) : BaseRepositoryExt<AiMessageEntity>(configuration, logger), IAiMessageRepository
{
}
