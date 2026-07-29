using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 待办事项仓储实现
/// </summary>
public class TodoItemRepository(IConfiguration configuration, ILogger<TodoItemRepository> logger) : BaseRepositoryExt<TodoItemEntity>(configuration, logger), ITodoItemRepository
{
    /// <inheritdoc />
    protected override bool IsLogicallyDelete => false;
}