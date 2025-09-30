using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class ParamRepository(IConfiguration configuration, ILogger<ParamRepository> logger) : BaseRepositoryExt<ParamEntity>(configuration, logger), IParamRepository
{

}