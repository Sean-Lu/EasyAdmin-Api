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
/// 字典类型管理
/// </summary>
public class SysDictTypeController(
    ILogger<SysDictTypeController> logger,
    IMapper mapper,
    ISysDictTypeService sysDictTypeService
    ) : BaseApiController
{
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

    [HttpPost]
    public async Task<ApiResult<bool>> Update(SysDictTypeDto data)
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

    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await sysDictTypeService.UpdateStateAsync(id, state));
    }

    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<SysDictTypeDto>>> Page([FromQuery] SysDictTypePageReqDto request)
    {
        var pageResult = await sysDictTypeService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<SysDictTypeDto>>(pageResult));
    }

    [HttpGet]
    public async Task<ApiResult<List<SysDictTypeDto>>> List()
    {
        return Success(mapper.Map<List<SysDictTypeDto>>(await sysDictTypeService.GetAllAsync()));
    }

    [HttpGet]
    public async Task<ApiResult<SysDictTypeDto>> Detail(long id)
    {
        return Success(mapper.Map<SysDictTypeDto>(await sysDictTypeService.GetByIdAsync(id)));
    }
}