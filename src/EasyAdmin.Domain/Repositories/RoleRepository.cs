using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 角色仓储实现
/// </summary>
public class RoleRepository(IConfiguration configuration, ILogger<RoleRepository> logger) : BaseRepositoryExt<RoleEntity>(configuration, logger), IRoleRepository
{

}