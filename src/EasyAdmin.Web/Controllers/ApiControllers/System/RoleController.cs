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
/// 角色管理
/// </summary>
public class RoleController(
    ILogger<RoleController> logger,
    IMapper mapper,
    IRoleService roleService
    ) : BaseApiController
{
    /// <summary>
    /// 新增角色
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(RoleDto data)
    {
        return Success(await roleService.AddAsync(data));
    }

    /// <summary>
    /// 删除角色
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
            return Success(await roleService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await roleService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改角色
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(RoleDto data)
    {
        return Success(await roleService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改角色状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await roleService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 角色分页列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<RoleDto>>> Page([FromQuery] RolePageReqDto request)
    {
        var pageResult = await roleService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<RoleDto>>(pageResult));
    }

    /// <summary>
    /// 查询所有角色
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<RoleDto>>> List()
    {
        var roles = await roleService.GetAllRolesAsync();
        return Success(mapper.Map<List<RoleDto>>(roles));
    }

    /// <summary>
    /// 查询角色详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<RoleDto>> Detail(long id)
    {
        return Success(mapper.Map<RoleDto>(await roleService.GetByIdAsync(id)));
    }

    /// <summary>
    /// 分配角色菜单权限
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> AssignMenus(RoleMenuAssignmentDto data)
    {
        return Success(await roleService.AssignMenusToRoleAsync(data));
    }

    /// <summary>
    /// 获取角色菜单权限ID列表
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<long>>> GetRoleMenuIds(long roleId)
    {
        return Success(await roleService.GetRoleMenuIdsAsync(roleId));
    }

    /// <summary>
    /// 获取角色菜单权限列表
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<RoleMenuDto>>> GetRoleMenus(long roleId)
    {
        var roleMenus = await roleService.GetRoleMenusAsync(roleId);
        return Success(mapper.Map<List<RoleMenuDto>>(roleMenus));
    }
}