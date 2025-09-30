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
/// 参数管理
/// </summary>
public class ParamController(
    ILogger<ParamController> logger,
    IMapper mapper,
    IParamService paramService
    ) : BaseApiController
{
    /// <summary>
    /// 新增参数
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(ParamDto data)
    {
        return Success(await paramService.AddAsync(data));
    }

    /// <summary>
    /// 删除参数
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
            return Success(await paramService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await paramService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改参数
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(ParamDto data)
    {
        return Success(await paramService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改参数状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await paramService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 分页查询参数列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<ParamDto>>> Page([FromQuery] ParamPageReqDto request)
    {
        var pageResult = await paramService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<ParamDto>>(pageResult));
    }

    /// <summary>
    /// 查询参数详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ParamDto>> Detail(long id)
    {
        return Success(mapper.Map<ParamDto>(await paramService.GetByIdAsync(id)));
    }
}