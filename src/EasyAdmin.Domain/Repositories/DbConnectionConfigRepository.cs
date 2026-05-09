using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class DbConnectionConfigRepository(IConfiguration configuration, ILogger<DbConnectionConfigRepository> logger) : BaseRepositoryExt<DbConnectionConfigEntity>(configuration, logger), IDbConnectionConfigRepository
{
}
