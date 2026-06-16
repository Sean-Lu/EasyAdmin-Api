using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 抽奖参与人仓库实现
/// </summary>
public class LotteryParticipantRepository(IConfiguration configuration, ILogger<LotteryParticipantRepository> logger)
    : BaseRepositoryExt<LotteryParticipantEntity>(configuration, logger), ILotteryParticipantRepository
{
}
