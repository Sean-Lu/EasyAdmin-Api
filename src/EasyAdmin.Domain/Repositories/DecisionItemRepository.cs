using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 随机决策候选项仓储实现
/// </summary>
public class DecisionItemRepository(IConfiguration configuration, ILogger<DecisionItemRepository> logger)
    : BaseRepositoryExt<DecisionItemEntity>(configuration, logger), IDecisionItemRepository
{
    /// <inheritdoc />
    protected override bool IsLogicallyDelete => false;
}
