using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class SysDictTypeRepository(IConfiguration configuration, ILogger<SysDictTypeRepository> logger) : BaseRepositoryExt<SysDictTypeEntity>(configuration, logger), ISysDictTypeRepository
{

}
