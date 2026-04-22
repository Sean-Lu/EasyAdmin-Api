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
/// 字典数据管理
/// </summary>
public class SysDictDataController(
    ILogger<SysDictDataController> logger,
    IMapper mapper,
    ISysDictDataService sysDictDataService
    ) : BaseApiController
{
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(SysDictDataDto data)
    {
        if (data.DictTypeId < 1)
        {
            return Fail<bool>("请选择字典类型");
        }
        if (string.IsNullOrEmpty(data.DictValue))
        {
            return Fail<bool>("字典值不能为空");
        }

        return Success(await sysDictDataService.AddAsync(data));
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            return Success(await sysDictDataService.DeleteByIdsAsync(ids));
        }

        var id = data["id"]?.Value<long>() ?? default;
        return Success(await sysDictDataService.DeleteByIdAsync(id));
    }

    [HttpPost]
    public async Task<ApiResult<bool>> Update(SysDictDataDto data)
    {
        if (data.DictTypeId < 1)
        {
            return Fail<bool>("请选择字典类型");
        }
        if (string.IsNullOrEmpty(data.DictValue))
        {
            return Fail<bool>("字典值不能为空");
        }

        return Success(await sysDictDataService.UpdateAsync(data));
    }

    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await sysDictDataService.UpdateStateAsync(id, state));
    }

    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<SysDictDataDto>>> Page([FromQuery] SysDictDataPageReqDto request)
    {
        var pageResult = await sysDictDataService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<SysDictDataDto>>(pageResult));
    }

    [HttpGet]
    public async Task<ApiResult<List<SysDictDataDto>>> GetByTypeCode(string typeCode)
    {
        return Success(mapper.Map<List<SysDictDataDto>>(await sysDictDataService.GetByTypeCodeAsync(typeCode)));
    }

    [HttpGet]
    public async Task<ApiResult<SysDictDataDto>> Detail(long id)
    {
        return Success(mapper.Map<SysDictDataDto>(await sysDictDataService.GetByIdAsync(id)));
    }
}