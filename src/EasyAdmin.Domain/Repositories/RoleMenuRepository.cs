using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class RoleMenuRepository(IConfiguration configuration, ILogger<RoleMenuRepository> logger) : BaseRepositoryExt<RoleMenuEntity>(configuration, logger), IRoleMenuRepository
{

}