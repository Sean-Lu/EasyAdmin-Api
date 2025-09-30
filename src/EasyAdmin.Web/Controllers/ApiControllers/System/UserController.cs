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
/// 用户管理
/// </summary>
public class UserController(
    ILogger<UserController> logger,
    IConfiguration configuration,
    IMapper mapper,
    IUserService userService
    ) : BaseApiController
{
    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(UserDto data)
    {
        if (string.IsNullOrEmpty(data.UserName))
        {
            return Fail<bool>("用户名称不能为空");
        }

        return Success(await userService.AddAsync(data));
    }

    /// <summary>
    /// 删除用户
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
            return Success(await userService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await userService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改用户
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(UserDto data)
    {
        return Success(await userService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改用户状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await userService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<UserDto>>> Page([FromQuery] UserPageReqDto request)
    {
        var pageResult = await userService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<UserDto>>(pageResult));
    }

    /// <summary>
    /// 查询用户列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<UserDto>>> List()
    {
        var pageResult = await userService.GetAllUserAsync();
        return Success(mapper.Map<List<UserDto>>(pageResult));
    }

    /// <summary>
    /// 查询用户详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<UserDto>> Detail(long id)
    {
        return Success(mapper.Map<UserDto>(await userService.GetByIdAsync(id)));
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<UserDto>> GetUserInfo()
    {
        return Success(mapper.Map<UserDto>(await userService.GetByIdAsync(UserId)));
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> ChangePassword([FromBody] JObject data)
    {
        var oldPassword = data["oldPassword"]?.Value<string>();
        var newPassword = data["newPassword"]?.Value<string>();
        if (!await userService.ChangePasswordAsync(UserId, oldPassword, newPassword))
        {
            return Fail<bool>("修改密码失败");
        }
        return Success(true);
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> ResetPassword([FromBody] JObject data)
    {
        //var ids = data["ids"]?.Values<long>().ToList() ?? default;
        var id = data["id"]?.Value<long>() ?? default;
        if (!await userService.ResetPasswordAsync(id))
        {
            return Fail<bool>("重置密码失败");
        }
        return Success(true);
    }
}