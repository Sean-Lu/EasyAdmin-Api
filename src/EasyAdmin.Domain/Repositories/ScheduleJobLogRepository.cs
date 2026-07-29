using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 定时任务日志仓储实现
/// </summary>
public class ScheduleJobLogRepository(IConfiguration configuration, ILogger<ScheduleJobLogRepository> logger) : BaseRepositoryExt<ScheduleJobLogEntity>(configuration, logger), IScheduleJobLogRepository
{

}