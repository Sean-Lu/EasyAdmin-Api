using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 笔记密码
/// </summary>
public class NotePasswordController(
    INotePasswordService notePasswordService
    ) : BaseApiController
{
    /// <summary>
    /// 状态
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<NotePasswordStatusDto>> Status()
    {
        return Success(await notePasswordService.GetStatusAsync());
    }

    /// <summary>
    /// 设置
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Set(NotePasswordSetDto data)
    {
        return Success(await notePasswordService.SetAsync(data));
    }

    /// <summary>
    /// 修改
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Change(NotePasswordChangeDto data)
    {
        return Success(await notePasswordService.ChangeAsync(data));
    }

    /// <summary>
    /// 验证
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<NotePasswordVerifyResultDto>> Verify(NotePasswordVerifyDto data)
    {
        return Success(await notePasswordService.VerifyAsync(data));
    }

    /// <summary>
    /// 重置
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Reset(NotePasswordVerifyDto data)
    {
        return Success(await notePasswordService.ResetAsync(data));
    }
}
