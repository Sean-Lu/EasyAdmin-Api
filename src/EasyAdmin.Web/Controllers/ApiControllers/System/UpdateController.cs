using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 更新管理
/// </summary>
public class UpdateController(
    ILogger<UpdateController> logger,
    IMapper mapper,
    IVersionService versionService
) : BaseApiController
{
    /// <summary>
    /// 注册新版本
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<long>> Register(
        [FromForm] string appCode,
        [FromForm] string versionName,
        [FromForm] string platform,
        [FromForm] string? changelog,
        [FromForm] bool isForceUpdate,
        [FromForm] int minSupportedVersionCode,
        [FromForm] UpdatePackageType updateType,
        IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return Fail<long>("请上传更新包文件");
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return Fail<long>("仅支持 .zip 格式的更新包");
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"EasyUpdate_Upload_{Guid.NewGuid():N}");
        var zipPath = Path.Combine(tempDir, file.FileName);

        try
        {
            Directory.CreateDirectory(tempDir);
            await using (var stream = file.OpenReadStream())
            await using (var fileStream = new FileStream(zipPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            var dto = new UpdateVersionAddDto
            {
                AppCode = appCode,
                VersionName = versionName,
                Platform = platform,
                Changelog = changelog,
                IsForceUpdate = isForceUpdate,
                MinSupportedVersionCode = minSupportedVersionCode,
                UpdateType = updateType
            };

            var id = await versionService.RegisterAsync(dto, zipPath);
            return Success(id);
        }
        catch (InvalidOperationException ex)
        {
            return Fail<long>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "注册版本失败");
            return Fail<long>($"注册版本失败: {ex.Message}");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    /// 删除版本：同时清理关联的文件存储
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        try
        {
            if (data == null)
            {
                return Fail<bool>("缺少参数");
            }

            var ids = data["ids"]?.Values<long>().ToList() ?? default;
            if (ids != null && ids.Any())
            {
                // 批量删除：遍历每个版本ID
                foreach (var id in ids)
                {
                    var success = await versionService.DeleteAsync(id);
                    if (!success)
                    {
                        return Fail<bool>($"删除版本失败: {id}");
                    }
                }
                return Success(true);
            }

            // 单个删除
            var singleId = data["id"]?.Value<long>() ?? default;
            if (singleId <= 0)
            {
                return Fail<bool>("缺少版本ID参数");
            }
            return Success(await versionService.DeleteAsync(singleId));
        }
        catch (InvalidOperationException ex)
        {
            return Fail<bool>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "删除版本失败");
            return Fail<bool>($"删除版本失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新版本元数据
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(UpdateVersionUpdateDto data)
    {
        try
        {
            return Success(await versionService.UpdateAsync(data));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新版本信息失败");
            return Fail<bool>($"更新版本信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 设置版本发布/回滚状态：同一平台只能有一个已发布版本，启用新版本会自动禁用旧的
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        try
        {
            return Success(await versionService.UpdateStateAsync(id, state));
        }
        catch (InvalidOperationException ex)
        {
            return Fail<bool>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "设置版本状态失败");
            return Fail<bool>($"设置版本状态失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 分页查询版本列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<UpdateVersionDto>>> Page([FromQuery] UpdateVersionPageReqDto request)
    {
        var pageResult = await versionService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<UpdateVersionDto>>(pageResult));
    }

    /// <summary>
    /// 查看版本详情
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<UpdateVersionDetailDto>> Detail(long id)
    {
        try
        {
            return Success(await versionService.DetailAsync(id));
        }
        catch (InvalidOperationException ex)
        {
            return Fail<UpdateVersionDetailDto>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "查看版本详情失败");
            return Fail<UpdateVersionDetailDto>($"查看版本详情失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 版本检测接口：客户端传入当前版本号查询是否需要更新
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResult<UpdateCheckResultDto>> Check([FromQuery] int currentVersionCode, [FromQuery] string appCode, [FromQuery] string platform)
    {
        if (currentVersionCode < 0)
        {
            return Fail<UpdateCheckResultDto>("版本号无效");
        }

        if (string.IsNullOrWhiteSpace(appCode))
        {
            return Fail<UpdateCheckResultDto>("应用标识不能为空");
        }

        if (string.IsNullOrWhiteSpace(platform))
        {
            return Fail<UpdateCheckResultDto>("平台标识不能为空");
        }

        try
        {
            var result = await versionService.CheckAsync(currentVersionCode, appCode, platform);
            return Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "版本检测失败");
            return Fail<UpdateCheckResultDto>($"版本检测失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取最新已发布版本信息
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResult<UpdateCheckResultDto>> LatestVersion([FromQuery] string appCode, [FromQuery] string platform)
    {
        if (string.IsNullOrWhiteSpace(appCode))
        {
            return Fail<UpdateCheckResultDto>("应用标识不能为空");
        }

        if (string.IsNullOrWhiteSpace(platform))
        {
            return Fail<UpdateCheckResultDto>("平台标识不能为空");
        }

        try
        {
            var result = await versionService.GetLatestAsync(appCode, platform);
            return Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取最新版本信息失败");
            return Fail<UpdateCheckResultDto>($"获取最新版本信息失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取增量更新清单：返回目标版本与当前版本的差异文件列表
    /// 跨版本场景会自动跳过中间版本，直接计算起点到终点的差异
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResult<UpdateManifestResultDto>> Manifest([FromQuery] int currentVersionCode, [FromQuery] int targetVersionCode, [FromQuery] string appCode, [FromQuery] string platform)
    {
        if (targetVersionCode <= 0)
        {
            return Fail<UpdateManifestResultDto>("目标版本号无效");
        }

        if (string.IsNullOrWhiteSpace(appCode))
        {
            return Fail<UpdateManifestResultDto>("应用标识不能为空");
        }

        if (string.IsNullOrWhiteSpace(platform))
        {
            return Fail<UpdateManifestResultDto>("平台标识不能为空");
        }

        try
        {
            var result = await versionService.GetManifestAsync(currentVersionCode, targetVersionCode, appCode, platform);
            return Success(result);
        }
        catch (InvalidOperationException ex)
        {
            return Fail<UpdateManifestResultDto>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取更新清单失败");
            return Fail<UpdateManifestResultDto>($"获取更新清单失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 下载单个更新文件：客户端按更新清单逐个下载文件
    /// 响应头包含 X-Checksum 供客户端下载后校验完整性
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> DownloadFile([FromQuery] int versionCode, [FromQuery] string filePath, [FromQuery] string appCode, [FromQuery] string platform)
    {
        if (versionCode <= 0)
        {
            return BadRequest("版本号无效");
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return BadRequest("文件路径不能为空");
        }

        if (string.IsNullOrWhiteSpace(appCode))
        {
            return BadRequest("应用标识不能为空");
        }

        if (string.IsNullOrWhiteSpace(platform))
        {
            return BadRequest("平台标识不能为空");
        }

        try
        {
            var (stream, checksum, fileName) = await versionService.GetFileStreamAsync(versionCode, filePath, appCode, platform);
            Response.Headers.Append("X-Checksum", checksum);
            Response.Headers.Append("X-File-Path", fileName);
            return File(stream, "application/octet-stream", Path.GetFileName(fileName));
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "下载文件失败: {FilePath}", filePath);
            return BadRequest($"下载文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 下载完整更新包zip
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> DownloadZip([FromQuery] long versionId)
    {
        if (versionId <= 0)
        {
            return BadRequest("版本ID无效");
        }

        try
        {
            var (zipStream, fileName) = await versionService.DownloadZipAsync(versionId);
            return File(zipStream, "application/zip", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "下载更新包失败");
            return BadRequest($"下载失败: {ex.Message}");
        }
    }
}