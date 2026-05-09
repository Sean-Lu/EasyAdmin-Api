using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers.ApiControllers.Tool;

/// <summary>
/// 代码生成分类管理
/// </summary>
public class CodeGenCategoryController(
    ILogger<CodeGenCategoryController> logger,
    ICodeGenCategoryService categoryService) : BaseApiController
{
    /// <summary>
    /// 获取分类树
    /// </summary>
    /// <returns>分类树列表</returns>
    [HttpGet]
    public async Task<ApiResult<List<CodeGenCategoryDto>>> Tree()
    {
        var result = await categoryService.GetTreeAsync();
        return Success(result);
    }

    /// <summary>
    /// 获取分类列表
    /// </summary>
    /// <returns>分类列表</returns>
    [HttpGet]
    public async Task<ApiResult<List<CodeGenCategoryDto>>> List()
    {
        var result = await categoryService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 获取分类详情
    /// </summary>
    /// <param name="id">分类ID</param>
    /// <returns>分类详情</returns>
    [HttpGet]
    public async Task<ApiResult<CodeGenCategoryDto>> Detail(long id)
    {
        var result = await categoryService.GetByIdAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 新增分类
    /// </summary>
    /// <param name="request">分类信息</param>
    /// <returns>分类ID</returns>
    [HttpPost]
    public async Task<ApiResult<long>> Add(CodeGenCategoryAddDto request)
    {
        var result = await categoryService.AddAsync(request);
        return Success(result);
    }

    /// <summary>
    /// 更新分类
    /// </summary>
    /// <param name="request">分类信息</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(CodeGenCategoryUpdateDto request)
    {
        await categoryService.UpdateAsync(request);
        return Success(true);
    }

    /// <summary>
    /// 删除分类
    /// </summary>
    /// <param name="data">包含ID的JSON对象</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        await categoryService.DeleteAsync(id);
        return Success(true);
    }

    /// <summary>
    /// 导出分类
    /// </summary>
    /// <returns>JSON文件流</returns>
    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var result = await categoryService.ExportAsync();
        return File(System.Text.Encoding.UTF8.GetBytes(result), "application/json", "categories.json");
    }

    /// <summary>
    /// 导入分类
    /// </summary>
    /// <param name="request">分类数据</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Import(CodeGenCategoryImportDto request)
    {
        await categoryService.ImportAsync(request);
        return Success(true);
    }
}