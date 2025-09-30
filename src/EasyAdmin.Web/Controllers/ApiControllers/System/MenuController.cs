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
/// 菜单管理
/// </summary>
public class MenuController(
    ILogger<MenuController> logger,
    IMapper mapper,
    IMenuService menuService
    ) : BaseApiController
{
    /// <summary>
    /// 新增菜单
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(MenuDto data)
    {
        return Success(await menuService.AddAsync(data));
    }

    /// <summary>
    /// 删除菜单
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
            return Success(await menuService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await menuService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改菜单
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(MenuDto data)
    {
        return Success(await menuService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改菜单状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await menuService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 查询菜单树
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<MenuDto>?>> ListTree([FromQuery] MenuListReqDto request)
    {
        // todo:菜单权限控制：不同用户看到的菜单列表不一样

        return Success(mapper.Map<List<MenuDto>>(await menuService.GetMenuTreeAsync(request)));
    }

    /// <summary>
    /// 查询菜单详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<MenuDto>> Detail(long id)
    {
        return Success(mapper.Map<MenuDto>(await menuService.GetByIdAsync(id)));
    }
}