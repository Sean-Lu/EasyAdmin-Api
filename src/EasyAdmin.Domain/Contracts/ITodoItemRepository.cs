using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 待办事项仓库接口
/// </summary>
public interface ITodoItemRepository : IBaseRepositoryExt<TodoItemEntity>
{

}