using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 任务管理
/// </summary>
public class TaskController(
    ILogger<TaskController> logger,
    IMapper mapper,
    ITaskService taskService
    ) : BaseApiController
{
    /// <summary>
    /// 新增任务
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(TaskDto data)
    {
        if (string.IsNullOrEmpty(data.TaskName))
        {
            return Fail<bool>("任务名称不能为空");
        }
        if (data.TaskType < 1)
        {
            return Fail<bool>("任务类型不能为空");
        }

        return Success(await taskService.AddAsync(data));
    }

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            // 批量删除
            return Success(await taskService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await taskService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改任务
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(TaskDto data)
    {
        return Success(await taskService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改任务状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await taskService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 分页查询任务列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<TaskDto>>> Page([FromQuery] TaskPageReqDto request)
    {
        var pageResult = await taskService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<TaskDto>>(pageResult));
    }

    /// <summary>
    /// 查询任务详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<TaskDto>> Detail(long id)
    {
        return Success(mapper.Map<TaskDto>(await taskService.GetByIdAsync(id)));
    }
}