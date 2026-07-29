using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 定时任务仓储实现
/// </summary>
public class ScheduleJobRepository(IConfiguration configuration, ILogger<ScheduleJobRepository> logger) : BaseRepositoryExt<ScheduleJobEntity>(configuration, logger), IScheduleJobRepository
{

}