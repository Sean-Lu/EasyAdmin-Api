using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 抽奖活动仓库实现
/// </summary>
public class LotteryActivityRepository(IConfiguration configuration, ILogger<LotteryActivityRepository> logger)
    : BaseRepositoryExt<LotteryActivityEntity>(configuration, logger), ILotteryActivityRepository
{
}
