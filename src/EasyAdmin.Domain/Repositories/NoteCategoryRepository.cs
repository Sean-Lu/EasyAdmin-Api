using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 笔记分类仓储实现
/// </summary>
public class NoteCategoryRepository(IConfiguration configuration, ILogger<NoteCategoryRepository> logger) : BaseRepositoryExt<NoteCategoryEntity>(configuration, logger), INoteCategoryRepository
{
}
