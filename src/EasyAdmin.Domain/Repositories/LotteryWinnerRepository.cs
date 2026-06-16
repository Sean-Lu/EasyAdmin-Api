using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 抽奖中奖记录仓库实现
/// </summary>
public class LotteryWinnerRepository(IConfiguration configuration, ILogger<LotteryWinnerRepository> logger)
    : BaseRepositoryExt<LotteryWinnerEntity>(configuration, logger), ILotteryWinnerRepository
{
}
