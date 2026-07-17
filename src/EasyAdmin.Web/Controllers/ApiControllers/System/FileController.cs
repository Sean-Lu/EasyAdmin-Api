using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Storage;
using EasyAdmin.Web.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 文件管理
/// </summary>
public class FileController(
    ILogger<FileController> logger,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    IMapper mapper,
    IFileService fileService,
    IFileStorageFactory fileStorageFactory
    ) : BaseApiController
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="storeType">存储类型</param>
    /// <param name="description"></param>
    /// <param name="bizType">业务分类</param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<FileDto>> UploadFile([FromForm] FileStoreType storeType, [FromForm] string? description, [FromForm] FileBizType bizType, IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return Fail<FileDto>("文件不能为空");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var extension = Path.GetExtension(file.FileName);
            var storageFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var relativePath = $"UploadFiles/{TenantId}/{UserId}/{storageFileName}";
            var filePath = await fileStorageFactory.GetFileStorage(storeType).UploadAsync(stream, relativePath);

            var fileDto = new FileDto
            {
                Name = file.FileName,
                Path = filePath,
                Size = file.Length,
                ContentType = file.ContentType,
                StoreType = storeType,
                BizType = bizType,
                Description = description
            };
            var id = await fileService.AddAndReturnIdAsync(fileDto);
            if (id < 1)
            {
                return Fail<FileDto>("保存文件失败");
            }
            fileDto.Id = id;

            return Success(fileDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "上传文件失败");
            return Fail<FileDto>($"上传文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> DownloadFile(long id)
    {
        var fileEntity = await fileService.GetByIdAsync(id);
        if (!IsCurrentUserFile(fileEntity))
        {
            return NotFound("文件不存在");
        }

        try
        {
            var fileStorage = fileStorageFactory.GetFileStorage(fileEntity.StoreType);
            var stream = await fileStorage.DownloadAsync(fileEntity.Path);
            return File(stream, fileEntity.ContentType, fileEntity.Name);
        }
        catch (FileNotFoundException)
        {
            return NotFound("文件不存在");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "下载文件失败");
            return BadRequest($"下载文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ApiResult<bool>> DeleteFile(long id)
    {
        var fileEntity = await fileService.GetByIdAsync(id);
        if (!IsCurrentUserFile(fileEntity))
        {
            return Fail<bool>("文件不存在");
        }

        if (!FileService.CanDeleteFromFileManager(fileEntity.BizType))
        {
            return Fail<bool>("该文件被业务引用，不能在文件管理中删除");
        }

        try
        {
            if (!await fileService.HasOtherActiveFileWithSamePathAsync(id, fileEntity.Path))
            {
                var fileStorage = fileStorageFactory.GetFileStorage(fileEntity.StoreType);
                await fileStorage.DeleteAsync(fileEntity.Path);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "删除存储文件失败");
        }

        return Success(await fileService.DeleteByIdFromFileManagerAsync(id));
    }

    /// <summary>
    /// 分页查询文件列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<FileDto>>> Page([FromQuery] FilePageReqDto request)
    {
        var pageResult = await fileService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<FileDto>>(pageResult));
    }

    /// <summary>
    /// 查看文件详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<FileDto>> Detail(long id)
    {
        var fileEntity = await fileService.GetByIdAsync(id);
        if (!IsCurrentUserFile(fileEntity))
        {
            return Fail<FileDto>("文件不存在");
        }
        return Success(mapper.Map<FileDto>(fileEntity));
    }

    private bool IsCurrentUserFile(FileEntity? fileEntity)
    {
        return FileManagerAccessPolicy.CanAccess(fileEntity, TenantId, UserId);
    }
}