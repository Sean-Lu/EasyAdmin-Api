using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 行政区划仓储实现
/// </summary>
public class RegionRepository(IConfiguration configuration, ILogger<RegionRepository> logger) : BaseRepositoryExt<RegionEntity>(configuration, logger), IRegionRepository
{

}
