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
/// 部门管理
/// </summary>
public class DepartmentController(
    ILogger<DepartmentController> logger,
    IMapper mapper,
    IDepartmentService departmentService
    ) : BaseApiController
{
    /// <summary>
    /// 新增部门
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(DepartmentDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("部门名称不能为空");
        }

        return Success(await departmentService.AddAsync(data));
    }

    /// <summary>
    /// 删除部门
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
            return Success(await departmentService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await departmentService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改部门
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Update(DepartmentDto data)
    {
        if (string.IsNullOrEmpty(data.Name))
        {
            return Fail<bool>("部门名称不能为空");
        }

        return Success(await departmentService.UpdateAsync(data));
    }

    /// <summary>
    /// 更新部门状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        return Success(await departmentService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 查询部门树
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<DepartmentDto>?>> ListTree([FromQuery] DepartmentListReqDto request)
    {
        return Success(mapper.Map<List<DepartmentDto>?>(await departmentService.GetDepartmentTreeAsync(request)));
    }

    /// <summary>
    /// 查询部门详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<DepartmentDto>> Detail(long id)
    {
        return Success(mapper.Map<DepartmentDto>(await departmentService.GetByIdAsync(id)));
    }
}
