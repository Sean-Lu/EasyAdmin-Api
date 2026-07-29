using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// AI草稿仓储实现
/// </summary>
public class AiDraftRepository(IConfiguration configuration, ILogger<AiDraftRepository> logger) : BaseRepositoryExt<AiDraftEntity>(configuration, logger), IAiDraftRepository
{
}
