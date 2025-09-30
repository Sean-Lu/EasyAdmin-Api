using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class DayWorkReportRepository(IConfiguration configuration, ILogger<DayWorkReportRepository> logger) : BaseRepositoryExt<DayWorkReportEntity>(configuration, logger), IDayWorkReportRepository
{

}