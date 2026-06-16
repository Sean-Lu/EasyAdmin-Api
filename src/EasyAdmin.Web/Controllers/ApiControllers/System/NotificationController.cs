using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 通知管理
/// </summary>
public class NotificationController(
    ILogger<NotificationController> logger,
    IMapper mapper,
    INotificationService notificationService
    ) : BaseApiController
{
    /// <summary>
    /// 新增并发送通知
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(NotificationDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Title))
        {
            return Fail<bool>("标题不能为空");
        }
        if (string.IsNullOrWhiteSpace(data.Content))
        {
            return Fail<bool>("内容不能为空");
        }
        if (!data.SendToAll &&
            (data.UserIds == null || !data.UserIds.Any()) &&
            (data.RoleIds == null || !data.RoleIds.Any()) &&
            (data.DepartmentIds == null || !data.DepartmentIds.Any()))
        {
            return Fail<bool>("请选择发送范围");
        }

        return Success(await notificationService.AddAsync(data));
    }

    /// <summary>
    /// 删除通知
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var ids = data?["ids"]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            return Success(await notificationService.DeleteByIdsAsync(ids));
        }

        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await notificationService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 更新通知状态
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data?["state"]?.Value<int>() ?? default);
        return Success(await notificationService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 通知分页列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<NotificationDto>>> Page([FromQuery] NotificationPageReqDto request)
    {
        var pageResult = await notificationService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<NotificationDto>>(pageResult));
    }

    /// <summary>
    /// 通知详情
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<NotificationDto>> Detail(long id)
    {
        return Success(mapper.Map<NotificationDto>(await notificationService.GetByIdAsync(id)));
    }
}
