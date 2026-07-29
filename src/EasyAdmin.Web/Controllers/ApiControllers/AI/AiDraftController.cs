using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// AI草稿
/// </summary>
public sealed class AiDraftController(IAiDraftService draftService) : BaseApiController
{
    /// <summary>
    /// 获取草稿
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<AiDraftDto>> Detail(long id)
    {
        return Success(await draftService.GetAsync(id));
    }

    /// <summary>
    /// 更新草稿
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<AiDraftDto>> Update(AiDraftUpdateDto request)
    {
        return Success(await draftService.UpdateAsync(request));
    }

    /// <summary>
    /// 删除草稿
    /// </summary>
    [HttpPost]
    public async Task<ApiResult> Delete(long id)
    {
        await draftService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 确认草稿
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<AiDraftConfirmResultDto>> Confirm(long id)
    {
        return Success(await draftService.ConfirmAsync(id));
    }
}