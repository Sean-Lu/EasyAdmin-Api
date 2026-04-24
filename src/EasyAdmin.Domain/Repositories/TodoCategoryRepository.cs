using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 待办事项分类仓库实现
/// </summary>
public class TodoCategoryRepository(IConfiguration configuration, ILogger<TodoCategoryRepository> logger) : BaseRepositoryExt<TodoCategoryEntity>(configuration, logger), ITodoCategoryRepository
{
    protected override bool IsLogicallyDelete => false;
}
