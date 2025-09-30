using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 操作日志
/// </summary>
public class OperateLogController(
    ILogger<OperateLogController> logger,
    IConfiguration configuration,
    IMapper mapper,
    IOperateLogService operateLogService
    ) : BaseApiController
{
    /// <summary>
    /// 删除操作日志
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
            return Success(await operateLogService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await operateLogService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 分页查询操作日志列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<OperateLogDto>>> Page([FromQuery] OperateLogPageReqDto request)
    {
        var pageResult = await operateLogService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<OperateLogDto>>(pageResult));
    }

    /// <summary>
    /// 查询操作日志详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<OperateLogDto>> Detail(long id)
    {
        return Success(mapper.Map<OperateLogDto>(await operateLogService.GetByIdAsync(id)));
    }
}