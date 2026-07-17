using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 收藏仓储实现
/// </summary>
public class FavoriteRepository(IConfiguration configuration, ILogger<FavoriteRepository> logger)
    : BaseRepositoryExt<FavoriteEntity>(configuration, logger), IFavoriteRepository
{
}
