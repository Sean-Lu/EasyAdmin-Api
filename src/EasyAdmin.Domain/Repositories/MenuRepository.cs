using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 菜单仓储实现
/// </summary>
public class MenuRepository(IConfiguration configuration, ILogger<MenuRepository> logger) : BaseRepositoryExt<MenuEntity>(configuration, logger), IMenuRepository
{

}