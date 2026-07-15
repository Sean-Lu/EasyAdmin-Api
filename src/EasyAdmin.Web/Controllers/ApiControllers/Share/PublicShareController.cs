using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Extensions;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 公开分享
/// </summary>
[AllowAnonymous]
public class PublicShareController(IShareService shareService) : BaseApiController
{
    /// <summary>
    /// 获取状态
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<PublicShareStatusDto>> Status(string shareCode)
    {
        return Success(await shareService.GetPublicStatusAsync(shareCode));
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<PublicShareVerifyResultDto>> Verify(PublicShareVerifyDto request)
    {
        return Success(await shareService.VerifyPasswordAsync(request, HttpContext.GetClientIp() ?? string.Empty));
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<PublicShareFileDto>> FileInfo(string shareCode)
    {
        return Success(await shareService.GetPublicFileAsync(shareCode, GetAccessToken()));
    }

    /// <summary>
    /// 获取笔记
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<PublicShareNoteDto>> Note(string shareCode)
    {
        return Success(await shareService.GetPublicNoteAsync(shareCode, GetAccessToken()));
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> File(string shareCode)
    {
        var file = await shareService.OpenPublicFileAsync(shareCode, GetAccessToken());
        return File(file.Content, file.ContentType ?? "application/octet-stream", file.FileName);
    }

    /// <summary>
    /// 获取笔记图片
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> NoteImage(string shareCode, long fileId)
    {
        var image = await shareService.OpenPublicNoteImageAsync(shareCode, fileId, GetAccessToken());
        return File(image.Content, image.ContentType ?? "application/octet-stream");
    }

    private string? GetAccessToken()
    {
        return Request.Headers["X-Share-Access-Token"].FirstOrDefault();
    }
}