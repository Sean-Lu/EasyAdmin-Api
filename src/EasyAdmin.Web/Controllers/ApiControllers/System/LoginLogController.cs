using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 登录日志
/// </summary>
public class LoginLogController(
    ILogger<LoginLogController> logger,
    IConfiguration configuration,
    IMapper mapper,
    ILoginLogService loginLogService
    ) : BaseApiController
{
    /// <summary>
    /// 删除登录日志
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
            return Success(await loginLogService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await loginLogService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 分页查询登录日志列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<LoginLogDto>>> Page([FromQuery] LoginLogPageReqDto request)
    {
        var pageResult = await loginLogService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<LoginLogDto>>(pageResult));
    }

    /// <summary>
    /// 查询登录日志详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<LoginLogDto>> Detail(long id)
    {
        return Success(mapper.Map<LoginLogDto>(await loginLogService.GetByIdAsync(id)));
    }
}