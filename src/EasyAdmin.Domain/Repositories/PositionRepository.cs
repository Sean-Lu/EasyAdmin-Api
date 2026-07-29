using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 岗位仓储实现
/// </summary>
public class PositionRepository(IConfiguration configuration, ILogger<PositionRepository> logger) : BaseRepositoryExt<PositionEntity>(configuration, logger), IPositionRepository
{

}
