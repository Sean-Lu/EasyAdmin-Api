using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class UserRepository(IConfiguration configuration, ILogger<UserRepository> logger) : BaseRepositoryExt<UserEntity>(configuration, logger), IUserRepository
{

}