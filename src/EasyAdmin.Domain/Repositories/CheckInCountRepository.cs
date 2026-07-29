using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 签到统计仓储实现
/// </summary>
public class CheckInCountRepository(IConfiguration configuration, ILogger<CheckInCountRepository> logger) : BaseRepositoryExt<CheckInCountEntity>(configuration, logger), ICheckInCountRepository
{

}