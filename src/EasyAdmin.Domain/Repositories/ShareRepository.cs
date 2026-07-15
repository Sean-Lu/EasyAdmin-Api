using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 分享仓储实现
/// </summary>
public class ShareRepository(IConfiguration configuration, ILogger<ShareRepository> logger)
    : BaseRepositoryExt<ShareEntity>(configuration, logger), IShareRepository
{
}