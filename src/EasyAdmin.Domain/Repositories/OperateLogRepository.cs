using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class OperateLogRepository(IConfiguration configuration, ILogger<OperateLogRepository> logger) : BaseRepositoryExt<OperateLogEntity>(configuration, logger), IOperateLogRepository
{

}