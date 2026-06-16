using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 抽奖奖项仓库实现
/// </summary>
public class LotteryPrizeRepository(IConfiguration configuration, ILogger<LotteryPrizeRepository> logger)
    : BaseRepositoryExt<LotteryPrizeEntity>(configuration, logger), ILotteryPrizeRepository
{
}
