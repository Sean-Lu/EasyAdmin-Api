using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class WeekWorkReportRepository(IConfiguration configuration, ILogger<WeekWorkReportRepository> logger) : BaseRepositoryExt<WeekWorkReportEntity>(configuration, logger), IWeekWorkReportRepository
{

}
