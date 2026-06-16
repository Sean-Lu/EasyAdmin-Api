using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 我的消息
/// </summary>
public class UserMessageController(
    ILogger<UserMessageController> logger,
    IMapper mapper,
    INotificationService notificationService
    ) : BaseApiController
{
    /// <summary>
    /// 当前用户消息分页列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<UserMessageDto>>> Page([FromQuery] UserMessagePageReqDto request)
    {
        var pageResult = await notificationService.UserMessagePageAsync(request);
        return Success(mapper.Map<ApiResultPageData<UserMessageDto>>(pageResult));
    }

    /// <summary>
    /// 当前用户消息详情
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<UserMessageDto>> Detail(long id)
    {
        var detail = await notificationService.GetUserMessageDetailAsync(id);
        if (detail == null)
        {
            return Fail<UserMessageDto>("消息不存在");
        }

        return Success(mapper.Map<UserMessageDto>(detail));
    }

    /// <summary>
    /// 当前用户未读数量
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<int>> UnreadCount()
    {
        return Success(await notificationService.GetUnreadCountAsync());
    }

    /// <summary>
    /// 最近未读消息
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<UserMessageDto>>> RecentUnread(int count = 5)
    {
        return Success(mapper.Map<List<UserMessageDto>>(await notificationService.GetRecentUnreadAsync(count)));
    }

    /// <summary>
    /// 标记已读
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> MarkRead([FromBody] JObject? data)
    {
        var ids = data?["ids"]?.Values<long>().ToList() ?? new List<long>();
        return Success(await notificationService.MarkReadAsync(ids));
    }

    /// <summary>
    /// 全部标记已读
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> MarkAllRead()
    {
        return Success(await notificationService.MarkAllReadAsync());
    }
}
