using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class ScheduleJobRepository(IConfiguration configuration, ILogger<ScheduleJobRepository> logger) : BaseRepositoryExt<ScheduleJobEntity>(configuration, logger), IScheduleJobRepository
{

}