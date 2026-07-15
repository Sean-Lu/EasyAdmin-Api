using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 分享管理
/// </summary>
public class ShareController(IShareService shareService) : BaseApiController
{
    /// <summary>
    /// 获取配置
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ShareConfigDto>> Config(ShareTargetType targetType, long targetId)
    {
        return Success(await shareService.GetConfigAsync(targetType, targetId));
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<ShareConfigDto>> Save(ShareSaveDto request)
    {
        return Success(await shareService.SaveAsync(request));
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<ShareConfigDto>> Toggle(ShareToggleDto request)
    {
        return Success(await shareService.SetEnabledAsync(request));
    }

    /// <summary>
    /// 重置链接
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<ShareConfigDto>> Regenerate(ShareTargetRequestDto request)
    {
        return Success(await shareService.RegenerateAsync(request));
    }
}