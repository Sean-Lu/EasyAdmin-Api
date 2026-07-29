using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 应用代码仓储实现
/// </summary>
public class AppCodeRepository(
    IConfiguration configuration,
    ILogger<AppCodeRepository> logger
    ) : BaseRepositoryExt<AppCodeEntity>(configuration, logger), IAppCodeRepository
{
}