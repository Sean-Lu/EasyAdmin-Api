using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Sean.Core.DbRepository;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// AI管理
/// </summary>
[SuperAdminOnly]
public sealed class AiAdminController(IAiAdminService aiAdminService) : BaseApiController
{
    /// <summary>
    /// 获取模型配置
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<AiModelConfigDto>> ModelConfig()
    {
        return Success(await aiAdminService.GetModelConfigAsync());
    }

    /// <summary>
    /// 更新模型配置
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<AiModelConfigDto>> UpdateModelConfig(AiModelConfigUpdateDto request)
    {
        return Success(await aiAdminService.UpdateModelConfigAsync(request));
    }

    /// <summary>
    /// 获取租户AI设置
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<AiTenantSettingDto>> TenantSetting(long tenantId)
    {
        return Success(await aiAdminService.GetTenantSettingAsync(tenantId));
    }

    /// <summary>
    /// 更新租户AI设置
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<AiTenantSettingDto>> UpdateTenantSetting(AiTenantSettingUpdateDto request)
    {
        return Success(await aiAdminService.UpdateTenantSettingAsync(request));
    }

    /// <summary>
    /// 分页查询AI用量
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<PageQueryResult<AiUsageDto>>> UsagePage(AiUsagePageReqDto request)
    {
        return Success(await aiAdminService.PageUsageAsync(request));
    }

    /// <summary>
    /// 测试模型连接
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<AiConnectionTestResultDto>> TestConnection()
    {
        return Success(await aiAdminService.TestConnectionAsync());
    }
}