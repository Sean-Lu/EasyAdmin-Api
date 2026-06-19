using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Web.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 应用标识管理
/// </summary>
public class AppCodeController(
    ILogger<AppCodeController> logger,
    IMapper mapper,
    IAppCodeService appCodeService
) : BaseApiController
{
    /// <summary>
    /// 新增应用标识
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Add(AppCodeAddDto data)
    {
        try
        {
            return Success(await appCodeService.AddAsync(data));
        }
        catch (ExplicitException ex)
        {
            return Fail<bool>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增应用标识失败");
            return Fail<bool>($"新增应用标识失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除应用标识
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        try
        {
            if (data == null)
            {
                return Fail<bool>("缺少参数");
            }

            var ids = data["ids"]?.Values<long>().ToList() ?? default;
            if (ids != null && ids.Any())
            {
                foreach (var id in ids)
                {
                    await appCodeService.DeleteByIdAsync(id);
                }
                return Success(true);
            }

            var singleId = data["id"]?.Value<long>() ?? default;
            if (singleId <= 0)
            {
                return Fail<bool>("缺少应用标识ID参数");
            }
            return Success(await appCodeService.DeleteByIdAsync(singleId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "删除应用标识失败");
            return Fail<bool>($"删除应用标识失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新应用标识
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(AppCodeUpdateDto data)
    {
        try
        {
            return Success(await appCodeService.UpdateAsync(data));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新应用标识失败");
            return Fail<bool>($"更新应用标识失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新应用标识状态
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        try
        {
            if (data == null)
            {
                return Fail<bool>("缺少参数");
            }

            var id = data["id"]?.Value<long>() ?? default;
            var state = (CommonState)(data["state"]?.Value<int>() ?? default);
            return Success(await appCodeService.UpdateStateAsync(id, state));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新应用标识状态失败");
            return Fail<bool>($"更新应用标识状态失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 分页查询应用标识
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<AppCodeDto>>> Page([FromQuery] AppCodePageReqDto request)
    {
        var pageResult = await appCodeService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<AppCodeDto>>(pageResult));
    }

    /// <summary>
    /// 查看应用标识详情
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<AppCodeDto>> Detail(long id)
    {
        try
        {
            var entity = await appCodeService.GetByIdAsync(id);
            if (entity == null || entity.IsDelete)
            {
                return Fail<AppCodeDto>("应用标识不存在");
            }
            return Success(mapper.Map<AppCodeDto>(entity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "查看应用标识详情失败");
            return Fail<AppCodeDto>($"查看应用标识详情失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取应用标识列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<AppCodeDto>>> ActiveList()
    {
        try
        {
            var list = await appCodeService.GetActiveListAsync();
            return Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取应用标识列表失败");
            return Fail<List<AppCodeDto>>($"获取应用标识列表失败: {ex.Message}");
        }
    }
}
