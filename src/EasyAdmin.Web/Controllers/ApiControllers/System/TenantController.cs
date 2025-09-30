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
/// 租户管理
/// </summary>
public class TenantController(
    ILogger<TenantController> logger,
    IMapper mapper,
    ITenantService tenantService
    ) : BaseApiController
{
    /// <summary>
    /// 新增租户
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(TenantDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("租户名称不能为空");
        }
        if (string.IsNullOrEmpty(data.AdminUserName))
        {
            return Fail<bool>("租管账号名称不能为空");
        }

        return Success(await tenantService.AddAsync(data));
    }

    /// <summary>
    /// 删除租户
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
            return Success(await tenantService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await tenantService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改租户
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(TenantDto data)
    {
        return Success(await tenantService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改租户状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await tenantService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 分页查询租户列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<TenantDto>>> Page([FromQuery] TenantPageReqDto request)
    {
        var pageResult = await tenantService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<TenantDto>>(pageResult));
    }

    /// <summary>
    /// 查询租户详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<TenantDto>> Detail(long id)
    {
        return Success(mapper.Map<TenantDto>(await tenantService.GetByIdAsync(id)));
    }

    /// <summary>
    /// 通过名称查询租户详情
    /// </summary>
    /// <param name="name">租户名称</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<TenantDto>> GetByName(string name)
    {
        return Success(mapper.Map<TenantDto>(await tenantService.GetByNameAsync(name)));
    }
}