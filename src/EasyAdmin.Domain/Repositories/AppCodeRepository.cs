using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class AppCodeRepository(
    IConfiguration configuration,
    ILogger<AppCodeRepository> logger
    ) : BaseRepositoryExt<AppCodeEntity>(configuration, logger), IAppCodeRepository
{
}