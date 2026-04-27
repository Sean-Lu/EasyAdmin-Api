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
/// 定时任务管理
/// </summary>
public class ScheduleJobController(
    ILogger<ScheduleJobController> logger,
    IMapper mapper,
    IScheduleJobService scheduleJobService,
    IScheduleJobLogService scheduleJobLogService
    ) : BaseApiController
{
    #region 定时任务

    /// <summary>
    /// 新增定时任务
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(ScheduleJobDto data)
    {
        var result = await scheduleJobService.AddAsync(data);
        return Success(result);
    }

    /// <summary>
    /// 删除定时任务
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            // 批量删除
            var result = await scheduleJobService.DeleteByIdsAsync(ids);
            return Success(result);
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        var singleResult = await scheduleJobService.DeleteByIdAsync(id);
        return Success(singleResult);
    }

    /// <summary>
    /// 修改定时任务
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(ScheduleJobUpdateDto data)
    {
        var result = await scheduleJobService.UpdateAsync(data);
        return Success(result);
    }

    /// <summary>
    /// 修改定时任务状态
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        var result = await scheduleJobService.UpdateStateAsync(id, state);
        return Success(result);
    }

    /// <summary>
    /// 立即执行定时任务
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> RunOnce([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await scheduleJobService.RunOnceAsync(id));
    }

    /// <summary>
    /// 分页查询定时任务列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<ScheduleJobDto>>> Page([FromQuery] ScheduleJobPageReqDto request)
    {
        var pageResult = await scheduleJobService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<ScheduleJobDto>>(pageResult));
    }

    /// <summary>
    /// 查询定时任务详情
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ScheduleJobDto>> Detail(long id)
    {
        return Success(mapper.Map<ScheduleJobDto>(await scheduleJobService.GetByIdAsync(id)));
    }

    #endregion

    #region 执行日志

    /// <summary>
    /// 删除执行日志
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> DeleteLog([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            // 批量删除
            return Success(await scheduleJobLogService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await scheduleJobLogService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 清空任务执行日志
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> ClearLog([FromBody] JObject? data)
    {
        var jobId = data["jobId"]?.Value<long>() ?? default;
        if (jobId > 0)
        {
            return Success(await scheduleJobLogService.DeleteByJobIdAsync(jobId));
        }
        return Success(false);
    }

    /// <summary>
    /// 分页查询执行日志列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<ScheduleJobLogDto>>> LogPage([FromQuery] ScheduleJobLogPageReqDto request)
    {
        var pageResult = await scheduleJobLogService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<ScheduleJobLogDto>>(pageResult));
    }

    /// <summary>
    /// 查询执行日志详情
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ScheduleJobLogDto>> LogDetail(long id)
    {
        return Success(mapper.Map<ScheduleJobLogDto>(await scheduleJobLogService.GetByIdAsync(id)));
    }

    #endregion
}
