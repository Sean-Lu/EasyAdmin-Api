using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 更新版本仓储实现
/// </summary>
public class UpdateVersionRepository(
    IConfiguration configuration,
    ILogger<UpdateVersionRepository> logger
    ) : BaseRepositoryExt<UpdateVersionEntity>(configuration, logger), IUpdateVersionRepository
{
}