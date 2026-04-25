using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class MonthWorkReportRepository(IConfiguration configuration, ILogger<MonthWorkReportRepository> logger) : BaseRepositoryExt<MonthWorkReportEntity>(configuration, logger), IMonthWorkReportRepository
{

}
