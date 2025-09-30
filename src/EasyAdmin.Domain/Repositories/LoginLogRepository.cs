using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class LoginLogRepository(IConfiguration configuration, ILogger<LoginLogRepository> logger) : BaseRepositoryExt<LoginLogEntity>(configuration, logger), ILoginLogRepository
{

}