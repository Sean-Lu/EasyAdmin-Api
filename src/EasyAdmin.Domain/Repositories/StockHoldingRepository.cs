using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 股票持仓仓储实现
/// </summary>
public class StockHoldingRepository(IConfiguration configuration, ILogger<StockHoldingRepository> logger)
    : BaseRepositoryExt<StockHoldingEntity>(configuration, logger), IStockHoldingRepository
{
}
