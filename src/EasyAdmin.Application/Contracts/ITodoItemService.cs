using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 待办事项服务接口
/// </summary>
public interface ITodoItemService
{
    /// <summary>
    /// 添加待办事项
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<bool> AddAsync(TodoItemDto dto);
    /// <summary>
    /// 删除待办事项
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteByIdAsync(long id);
    /// <summary>
    /// 清除已完成的待办事项
    /// </summary>
    /// <param name="categoryId">分类ID（可选）</param>
    /// <returns></returns>
    Task<bool> ClearCompletedAsync(long? categoryId = null);
    /// <summary>
    /// 更新待办事项
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(TodoItemDto dto);
    /// <summary>
    /// 更新待办事项状态
    /// </summary>
    /// <param name="id"></param>
    /// <param name="done"></param>
    /// <returns></returns>
    Task<bool> UpdateStatusAsync(long id, bool done);
    /// <summary>
    /// 批量更新待办事项状态
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="done"></param>
    /// <returns></returns>
    Task<bool> UpdateStatusAsync(List<long> ids, bool done);
    /// <summary>
    /// 更新待办事项优先级
    /// </summary>
    /// <param name="id"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    Task<bool> UpdatePriorityAsync(long id, int priority);
    /// <summary>
    /// 更新待办事项内容
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<bool> UpdateNameAsync(long id, string name);
    /// <summary>
    /// 更新待办事项排序顺序
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sortOrder"></param>
    /// <returns></returns>
    Task<bool> UpdateSortOrderAsync(long id, int sortOrder);
    /// <summary>
    /// 根据ID获取待办事项
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TodoItemDto> GetByIdAsync(long id);
    /// <summary>
    /// 根据用户ID获取待办事项列表
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    Task<List<TodoItemDto>> GetByUserIdAsync(long categoryId);
}