using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 待办事项管理
/// </summary>
public class TodoItemController(
    ILogger<TodoItemController> logger,
    ITodoItemService todoItemService
    ) : BaseApiController
{
    /// <summary>
    /// 新增待办事项
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(TodoItemDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("待办事项名称不能为空");
        }

        return Success(await todoItemService.AddAsync(data));
    }

    /// <summary>
    /// 删除待办事项
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await todoItemService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 清除已完成的待办事项
    /// </summary>
    /// <param name="categoryId">分类ID（可选）</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> ClearCompleted(long? categoryId = null)
    {
        return Success(await todoItemService.ClearCompletedAsync(categoryId));
    }

    /// <summary>
    /// 修改待办事项
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(TodoItemDto data)
    {
        return Success(await todoItemService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改待办事项状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateStatus([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var done = data["done"]?.Value<bool>() ?? default;
        return Success(await todoItemService.UpdateStatusAsync(id, done));
    }

    /// <summary>
    /// 批量更新待办事项状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> BatchUpdateStatus([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        var done = data["done"]?.Value<bool>() ?? default;
        return Success(await todoItemService.UpdateStatusAsync(ids, done));
    }

    /// <summary>
    /// 更新待办事项优先级
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdatePriority([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var priority = data["priority"]?.Value<int>() ?? default;
        return Success(await todoItemService.UpdatePriorityAsync(id, priority));
    }

    /// <summary>
    /// 更新待办事项内容
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateName([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var name = data["name"]?.Value<string>() ?? string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            return Fail<bool>("待办事项内容不能为空");
        }
        return Success(await todoItemService.UpdateNameAsync(id, name));
    }

    /// <summary>
    /// 更新待办事项排序顺序
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateSortOrder([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var sortOrder = data["sortOrder"]?.Value<int>() ?? default;
        return Success(await todoItemService.UpdateSortOrderAsync(id, sortOrder));
    }

    /// <summary>
    /// 更新待办事项分类
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateCategory([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var categoryId = data["categoryId"]?.Value<long>() ?? default;
        return Success(await todoItemService.UpdateCategoryAsync(id, categoryId));
    }

    /// <summary>
    /// 获取待办事项列表
    /// </summary>
    /// <param name="categoryId">分类ID（可选）</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<TodoItemDto>>> List(long? categoryId = null)
    {
        return Success(await todoItemService.GetByUserIdAsync(categoryId.GetValueOrDefault()));
    }
}