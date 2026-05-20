using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class UpdateVersionRepository(
    IConfiguration configuration,
    ILogger<UpdateVersionRepository> logger
    ) : BaseRepositoryExt<UpdateVersionEntity>(configuration, logger), IUpdateVersionRepository
{
}