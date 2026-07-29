using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 数据库连接配置仓储实现
/// </summary>
public class DbConnectionConfigRepository(IConfiguration configuration, ILogger<DbConnectionConfigRepository> logger) : BaseRepositoryExt<DbConnectionConfigEntity>(configuration, logger), IDbConnectionConfigRepository
{
}
