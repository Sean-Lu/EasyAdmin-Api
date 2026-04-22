using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class SysDictDataRepository(IConfiguration configuration, ILogger<SysDictDataRepository> logger) : BaseRepositoryExt<SysDictDataEntity>(configuration, logger), ISysDictDataRepository
{

}
