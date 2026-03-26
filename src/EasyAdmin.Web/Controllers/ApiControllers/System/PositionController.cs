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
/// 岗位管理
/// </summary>
public class PositionController(
    ILogger<PositionController> logger,
    IMapper mapper,
    IPositionService positionService
    ) : BaseApiController
{
    /// <summary>
    /// 新增岗位
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(PositionDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("岗位名称不能为空");
        }
        if (string.IsNullOrEmpty(data.Code))
        {
            return Fail<bool>("岗位编码不能为空");
        }

        return Success(await positionService.AddAsync(data));
    }

    /// <summary>
    /// 删除岗位
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
            return Success(await positionService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await positionService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改岗位
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Update(PositionDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("岗位名称不能为空");
        }
        if (string.IsNullOrEmpty(data.Code))
        {
            return Fail<bool>("岗位编码不能为空");
        }

        return Success(await positionService.UpdateAsync(data));
    }

    /// <summary>
    /// 更新岗位状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await positionService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 分页查询岗位
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<PositionDto>>> Page([FromQuery] PositionPageReqDto request)
    {
        var result = await positionService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<PositionDto>>(result));
    }

    /// <summary>
    /// 查询岗位列表（不分页）
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<PositionDto>>> List()
    {
        return Success(mapper.Map<List<PositionDto>>(await positionService.GetListAsync()));
    }

    /// <summary>
    /// 查询岗位详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<PositionDto>> Detail(long id)
    {
        return Success(mapper.Map<PositionDto>(await positionService.GetByIdAsync(id)));
    }
}
