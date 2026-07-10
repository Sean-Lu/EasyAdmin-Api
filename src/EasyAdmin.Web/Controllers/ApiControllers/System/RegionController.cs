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
/// 行政区划
/// </summary>
public class RegionController(
    ILogger<RegionController> logger,
    IMapper mapper,
    IRegionService regionService
    ) : BaseApiController
{
    /// <summary>
    /// 新增行政区划
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(RegionDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("行政区划名称不能为空");
        }

        return Success(await regionService.AddAsync(data));
    }

    /// <summary>
    /// 删除行政区划
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
            return Success(await regionService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await regionService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改行政区划
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Update(RegionUpdateDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("行政区划名称不能为空");
        }

        return Success(await regionService.UpdateAsync(data));
    }

    /// <summary>
    /// 更新行政区划状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await regionService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 查询行政区划树
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<RegionDto>?>> ListTree([FromQuery] RegionListReqDto request)
    {
        return Success(mapper.Map<List<RegionDto>?>(await regionService.GetRegionTreeAsync(request)));
    }

    /// <summary>
    /// 查询行政区划详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<RegionDto>> Detail(long id)
    {
        return Success(mapper.Map<RegionDto>(await regionService.GetByIdAsync(id)));
    }
}
