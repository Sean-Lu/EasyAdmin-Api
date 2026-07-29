using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// AI会话仓储实现
/// </summary>
public class AiConversationRepository(IConfiguration configuration, ILogger<AiConversationRepository> logger) : BaseRepositoryExt<AiConversationEntity>(configuration, logger), IAiConversationRepository
{
}
