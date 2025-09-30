using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 字典管理
/// </summary>
public class SysDictController(
    ILogger<SysDictController> logger,
    IMapper mapper,
    ISysDictService sysDictService
    ) : BaseApiController
{
    /// <summary>
    /// 新增字典
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(SysDictDto data)
    {
        return Success(await sysDictService.AddAsync(data));
    }

    /// <summary>
    /// 删除字典
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
            return Success(await sysDictService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await sysDictService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改字典
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(SysDictDto data)
    {
        return Success(await sysDictService.UpdateAsync(data));
    }

    /// <summary>
    /// 分页查询字典列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<SysDictDto>>> Page([FromQuery] SysDictPageReqDto request)
    {
        var pageResult = await sysDictService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<SysDictDto>>(pageResult));
    }

    /// <summary>
    /// 查询字典详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<SysDictDto>> Detail(long id)
    {
        return Success(mapper.Map<SysDictDto>(await sysDictService.GetByIdAsync(id)));
    }
}