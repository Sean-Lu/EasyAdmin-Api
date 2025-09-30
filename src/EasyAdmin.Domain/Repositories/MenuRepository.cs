using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class MenuRepository(IConfiguration configuration, ILogger<MenuRepository> logger) : BaseRepositoryExt<MenuEntity>(configuration, logger), IMenuRepository
{

}