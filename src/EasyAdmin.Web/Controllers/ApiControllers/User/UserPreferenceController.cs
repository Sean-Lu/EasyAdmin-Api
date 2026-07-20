using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 用户偏好
/// </summary>
public class UserPreferenceController(IUserPreferenceService userPreferenceService) : BaseApiController
{
    /// <summary>
    /// 获取百宝箱工具排序
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ToolboxToolOrderDto>> GetToolboxToolOrder()
    {
        return Success(await userPreferenceService.GetToolboxToolOrderAsync());
    }

    /// <summary>
    /// 更新百宝箱工具排序
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<ToolboxToolOrderDto>> UpdateToolboxToolOrder(ToolboxToolOrderDto request)
    {
        return Success(await userPreferenceService.UpdateToolboxToolOrderAsync(request));
    }
}
