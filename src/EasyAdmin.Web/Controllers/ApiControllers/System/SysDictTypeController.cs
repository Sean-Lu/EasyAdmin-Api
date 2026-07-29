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
/// 字典类型管理
/// </summary>
public class SysDictTypeController(
    ILogger<SysDictTypeController> logger,
    IMapper mapper,
    ISysDictTypeService sysDictTypeService
    ) : BaseApiController
{
    /// <summary>
    /// 新增字典类型
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(SysDictTypeDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("字典类型名称不能为空");
        }
        if (string.IsNullOrEmpty(data.Code))
        {
            return Fail<bool>("字典类型编码不能为空");
        }

        return Success(await sysDictTypeService.AddAsync(data));
    }

    /// <summary>
    /// 删除字典类型
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            return Success(await sysDictTypeService.DeleteByIdsAsync(ids));
        }

        var id = data["id"]?.Value<long>() ?? default;
        return Success(await sysDictTypeService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 更新字典类型
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(SysDictTypeUpdateDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("字典类型名称不能为空");
        }
        if (string.IsNullOrEmpty(data.Code))
        {
            return Fail<bool>("字典类型编码不能为空");
        }

        return Success(await sysDictTypeService.UpdateAsync(data));
    }

    /// <summary>
    /// 更新字典类型状态
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await sysDictTypeService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 分页查询字典类型
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<SysDictTypeDto>>> Page([FromQuery] SysDictTypePageReqDto request)
    {
        var pageResult = await sysDictTypeService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<SysDictTypeDto>>(pageResult));
    }

    /// <summary>
    /// 查询字典类型列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<SysDictTypeDto>>> List()
    {
        return Success(mapper.Map<List<SysDictTypeDto>>(await sysDictTypeService.GetAllAsync()));
    }

    /// <summary>
    /// 查询字典类型详情
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<SysDictTypeDto>> Detail(long id)
    {
        return Success(mapper.Map<SysDictTypeDto>(await sysDictTypeService.GetByIdAsync(id)));
    }
}