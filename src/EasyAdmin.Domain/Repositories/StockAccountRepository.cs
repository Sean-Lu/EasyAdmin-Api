using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

public class StockAccountRepository(IConfiguration configuration, ILogger<StockAccountRepository> logger)
    : BaseRepositoryExt<StockAccountEntity>(configuration, logger), IStockAccountRepository
{
}
