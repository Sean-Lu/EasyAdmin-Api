using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 待办事项分类管理
/// </summary>
public class TodoCategoryController(
    ILogger<TodoCategoryController> logger,
    ITodoCategoryService todoCategoryService
    ) : BaseApiController
{
    /// <summary>
    /// 新增分类
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(TodoCategoryDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("分类名称不能为空");
        }

        return Success(await todoCategoryService.AddAsync(data));
    }

    /// <summary>
    /// 删除分类
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await todoCategoryService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改分类
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(TodoCategoryUpdateDto data)
    {
        return Success(await todoCategoryService.UpdateAsync(data));
    }

    /// <summary>
    /// 获取分类列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<TodoCategoryDto>>> List()
    {
        return Success(await todoCategoryService.GetByUserIdAsync());
    }
}
