using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 笔记标签
/// </summary>
public class NoteTagController(
    INoteTagService noteTagService
    ) : BaseApiController
{
    /// <summary>
    /// 列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<NoteTagDto>>> List()
    {
        return Success(await noteTagService.GetByUserIdAsync());
    }

    /// <summary>
    /// 建议
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<NoteTagDto>>> Suggest(string keyword = "")
    {
        return Success(await noteTagService.SuggestAsync(keyword));
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await noteTagService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 清理未使用
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> DeleteUnused()
    {
        return Success(await noteTagService.DeleteUnusedAsync());
    }
}
