using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Sean.Core.DbRepository;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 我的笔记
/// </summary>
public class NoteController(
    INoteService noteService,
    INoteExportService noteExportService
    ) : BaseApiController
{
    /// <summary>
    /// 分页
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<PageQueryResult<NoteDto>>> Page(NotePageReqDto request)
    {
        return Success(await noteService.PageAsync(request));
    }

    /// <summary>
    /// 详情
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<NoteDto?>> Detail(long id, string? unlockToken = null)
    {
        return Success(await noteService.GetDetailAsync(id, unlockToken));
    }

    /// <summary>
    /// 新增
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(NoteUpdateDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
        {
            return Fail<bool>("笔记标题不能为空");
        }
        return Success(await noteService.AddAsync(data));
    }

    /// <summary>
    /// 修改
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(NoteUpdateDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
        {
            return Fail<bool>("笔记标题不能为空");
        }
        return Success(await noteService.UpdateAsync(data));
    }

    /// <summary>
    /// 删除
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await noteService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> BatchDelete([FromBody] JObject? data)
    {
        var ids = data?["ids"]?.Values<long>().ToList() ?? new List<long>();
        return Success(await noteService.BatchDeleteAsync(ids));
    }

    /// <summary>
    /// 删除未引用图片
    /// </summary>
    [HttpDelete]
    public async Task<ApiResult<bool>> DeleteImageFile(long id)
    {
        return Success(await noteService.DeleteImageFileAsync(id));
    }

    /// <summary>
    /// 置顶
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateTop([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        var isTop = data?["isTop"]?.Value<bool>() ?? default;
        return Success(await noteService.UpdateTopAsync(id, isTop));
    }

    /// <summary>
    /// 移动分类
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> MoveCategory([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        var ids = data?["ids"]?.Values<long>().ToList() ?? new List<long>();
        var categoryId = data?["categoryId"]?.Value<long>() ?? default;
        if (ids.Count == 0 && id > 0)
        {
            ids.Add(id);
        }
        return Success(await noteService.MoveCategoryAsync(ids, categoryId));
    }

    /// <summary>
    /// 导出
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Export(NoteExportReqDto request)
    {
        var note = await noteService.GetDetailAsync(request.Id, request.UnlockToken);
        if (note == null)
        {
            return NotFound();
        }

        var file = await noteExportService.ExportAsync(note, request.ExportType);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
