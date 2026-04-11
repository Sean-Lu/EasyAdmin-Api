using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class UserRoleRepository(IConfiguration configuration, ILogger<UserRoleRepository> logger) : BaseRepositoryExt<UserRoleEntity>(configuration, logger), IUserRoleRepository
{

}