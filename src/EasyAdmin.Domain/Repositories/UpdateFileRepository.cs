using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 更新文件仓储实现
/// </summary>
public class UpdateFileRepository(
    IConfiguration configuration,
    ILogger<UpdateFileRepository> logger
    ) : BaseRepositoryExt<UpdateFileEntity>(configuration, logger), IUpdateFileRepository
{
}