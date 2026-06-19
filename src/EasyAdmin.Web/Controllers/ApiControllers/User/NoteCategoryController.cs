using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 笔记分类
/// </summary>
public class NoteCategoryController(
    INoteCategoryService noteCategoryService
    ) : BaseApiController
{
    /// <summary>
    /// 新增
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(NoteCategoryDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("分类名称不能为空");
        }
        return Success(await noteCategoryService.AddAsync(data));
    }

    /// <summary>
    /// 删除
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await noteCategoryService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(NoteCategoryUpdateDto data)
    {
        return Success(await noteCategoryService.UpdateAsync(data));
    }

    /// <summary>
    /// 排序
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateSortOrder([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        var sortOrder = data?["sortOrder"]?.Value<int>() ?? default;
        return Success(await noteCategoryService.UpdateSortOrderAsync(id, sortOrder));
    }

    /// <summary>
    /// 列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<NoteCategoryDto>>> List()
    {
        return Success(await noteCategoryService.GetByUserIdAsync());
    }
}
