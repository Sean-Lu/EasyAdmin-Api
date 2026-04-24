using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 待办事项分类服务接口
/// </summary>
public interface ITodoCategoryService
{
    /// <summary>
    /// 添加分类
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<bool> AddAsync(TodoCategoryDto dto);
    /// <summary>
    /// 删除分类
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteByIdAsync(long id);
    /// <summary>
    /// 更新分类
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(TodoCategoryUpdateDto dto);
    /// <summary>
    /// 根据ID获取分类
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TodoCategoryDto> GetByIdAsync(long id);
    /// <summary>
    /// 获取用户的所有分类
    /// </summary>
    /// <returns></returns>
    Task<List<TodoCategoryDto>> GetByUserIdAsync();
}
