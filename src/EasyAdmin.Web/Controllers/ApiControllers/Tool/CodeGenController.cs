using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers.ApiControllers.Tool;

/// <summary>
/// 代码生成
/// </summary>
public class CodeGenController(
    ILogger<CodeGenController> logger,
    IMapper mapper,
    ICodeGenService codeGenService
    ) : BaseApiController
{
    #region 模板管理

    /// <summary>
    /// 获取代码模板列表
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>模板列表</returns>
    [HttpGet]
    public async Task<ApiResult<List<CodeGenTemplateDto>>> GetTemplates([FromQuery] CodeGenTemplateListReqDto request)
    {
        var result = await codeGenService.GetTemplateListAsync(request);
        return Success(result);
    }

    /// <summary>
    /// 获取单个模板详情
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <returns>模板详情</returns>
    [HttpGet]
    public async Task<ApiResult<CodeGenTemplateDto>> GetTemplate(long id)
    {
        var result = await codeGenService.GetTemplateByIdAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 新增代码模板
    /// </summary>
    /// <param name="data">模板信息</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> AddTemplate(CodeGenTemplateDto data)
    {
        data.CreateUserId = UserId;
        data.UpdateUserId = UserId;
        var result = await codeGenService.AddTemplateAsync(data);
        return Success(result);
    }

    /// <summary>
    /// 更新代码模板
    /// </summary>
    /// <param name="data">模板信息</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateTemplate(CodeGenTemplateUpdateDto data)
    {
        var result = await codeGenService.UpdateTemplateAsync(data);
        return Success(result);
    }

    /// <summary>
    /// 删除代码模板
    /// </summary>
    /// <param name="data">包含ID的JSON对象</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> DeleteTemplate([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var result = await codeGenService.DeleteTemplateAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 批量删除代码模板
    /// </summary>
    /// <param name="data">包含ID列表的JSON对象</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> DeleteTemplates([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        var result = await codeGenService.DeleteTemplatesAsync(ids);
        return Success(result);
    }

    #endregion

    #region 数据库配置管理

    /// <summary>
    /// 获取数据库配置列表
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>配置列表</returns>
    [HttpGet]
    public async Task<ApiResult<List<DbConnectionConfigDto>>> GetDbConfigs([FromQuery] DbConnectionConfigListReqDto request)
    {
        var result = await codeGenService.GetDbConfigListAsync(request);
        return Success(result);
    }

    /// <summary>
    /// 获取单个数据库配置详情
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns>配置详情</returns>
    [HttpGet]
    public async Task<ApiResult<DbConnectionConfigDto>> GetDbConfig(long id)
    {
        var result = await codeGenService.GetDbConfigByIdAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 新增数据库配置
    /// </summary>
    /// <param name="data">配置信息</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> AddDbConfig(DbConnectionConfigUpdateDto data)
    {
        var result = await codeGenService.AddDbConfigAsync(data);
        return Success(result);
    }

    /// <summary>
    /// 更新数据库配置
    /// </summary>
    /// <param name="data">配置信息</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateDbConfig(DbConnectionConfigUpdateDto data)
    {
        var result = await codeGenService.UpdateDbConfigAsync(data);
        return Success(result);
    }

    /// <summary>
    /// 删除数据库配置
    /// </summary>
    /// <param name="data">包含ID的JSON对象</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> DeleteDbConfig([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var result = await codeGenService.DeleteDbConfigAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 批量删除数据库配置
    /// </summary>
    /// <param name="data">包含ID列表的JSON对象</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> DeleteDbConfigs([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        var result = await codeGenService.DeleteDbConfigsAsync(ids);
        return Success(result);
    }

    /// <summary>
    /// 测试数据库连接
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns>是否成功</returns>
    [HttpPost]
    public async Task<ApiResult<bool>> TestDbConnection(long id)
    {
        var result = await codeGenService.TestDbConnectionAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 获取数据库表列表
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns>表列表</returns>
    [HttpGet]
    public async Task<ApiResult<List<DbTableInfoDto>>> GetDbTables(long id)
    {
        var result = await codeGenService.GetDbTablesAsync(id);
        return Success(result);
    }

    #endregion

    #region 代码生成

    /// <summary>
    /// 生成代码（数据库模式）
    /// </summary>
    /// <param name="data">生成参数</param>
    /// <returns>生成结果</returns>
    [HttpPost]
    public async Task<ApiResult<CodeGenResultDto>> GenerateCode(CodeGenReqDto data)
    {
        var result = await codeGenService.GenerateCodeAsync(data);
        return Success(result);
    }

    /// <summary>
    /// 生成代码（配置模式 / CodeFirst解析模式）
    /// </summary>
    /// <param name="data">配置参数</param>
    /// <returns>生成结果</returns>
    [HttpPost]
    public async Task<ApiResult<CodeGenResultDto>> GenerateCodeByConfig(CodeGenConfigReqDto data)
    {
        var result = await codeGenService.GenerateCodeByConfigAsync(data);
        return Success(result);
    }

    /// <summary>
    /// 解析Entity源码并提取元数据
    /// </summary>
    /// <param name="data">源码和语言类型</param>
    /// <returns>解析后的实体元数据</returns>
    [HttpPost]
    public async Task<ApiResult<CodeFirstParseResultDto>> ParseEntityCode(CodeFirstParseReqDto data)
    {
        try
        {
            var result = await codeGenService.ParseEntityCodeAsync(data);
            return Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "解析Entity源码失败");
            return Fail<CodeFirstParseResultDto>(ex.Message);
        }
    }

    /// <summary>
    /// 下载单个代码文件
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <param name="fileName">文件名</param>
    /// <returns>文件流</returns>
    [HttpGet]
    public async Task<IActionResult> DownloadFile(string taskId, string fileName)
    {
        var fileBytes = await codeGenService.DownloadFileAsync(taskId, fileName);
        return File(fileBytes, "application/octet-stream", fileName);
    }

    /// <summary>
    /// 打包下载所有代码文件
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>ZIP文件流</returns>
    [HttpGet]
    public async Task<IActionResult> DownloadPackage(string taskId)
    {
        var fileBytes = await codeGenService.DownloadPackageAsync(taskId);
        return File(fileBytes, "application/zip", $"codegen_{DateTime.Now:yyyyMMddHHmmss}.zip");
    }

    #endregion
}