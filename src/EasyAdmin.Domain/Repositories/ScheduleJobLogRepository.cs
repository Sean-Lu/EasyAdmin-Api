using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class ScheduleJobLogRepository(IConfiguration configuration, ILogger<ScheduleJobLogRepository> logger) : BaseRepositoryExt<ScheduleJobLogEntity>(configuration, logger), IScheduleJobLogRepository
{

}