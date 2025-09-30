using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class SysDictRepository(IConfiguration configuration, ILogger<SysDictRepository> logger) : BaseRepositoryExt<SysDictEntity>(configuration, logger), ISysDictRepository
{

}