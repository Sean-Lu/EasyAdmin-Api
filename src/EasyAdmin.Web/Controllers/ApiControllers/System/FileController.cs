using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Models;
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
    IFileService fileService
    ) : BaseApiController
{
    private const string RootPath = "${RootPath}";// environment.ContentRootPath

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="description"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost]
    //[Consumes("multipart/form-data")]
    public async Task<ApiResult<FileDto>> UploadFile([FromForm] string? description, IFormFile? file)
    {
        //var file = Request.Form.Files.FirstOrDefault();
        //var description = Request.Form["description"].FirstOrDefault();

        if (file == null || file.Length == 0)
        {
            return Fail<FileDto>("文件不能为空");
        }

        var uploadFilePath = Path.Combine(environment.ContentRootPath, "UploadFiles", TenantId > 0 ? TenantId.ToString() : string.Empty, UserId > 0 ? UserId.ToString() : string.Empty);
        if (!Directory.Exists(uploadFilePath))
        {
            Directory.CreateDirectory(uploadFilePath);
        }

        var fileName = file.FileName;
        var filePath = Path.Combine(uploadFilePath, fileName);
        if (System.IO.File.Exists(filePath))
        {
            var extension = Path.GetExtension(fileName);
            filePath = Path.Combine(uploadFilePath, $"{Guid.NewGuid()}{extension}");
        }

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (filePath.StartsWith(environment.ContentRootPath))
        {
            filePath = $"{RootPath}{filePath.Substring(environment.ContentRootPath.Length)}";
        }
        var fileDto = new FileDto
        {
            Name = fileName,
            Path = filePath,
            Size = file.Length,
            ContentType = file.ContentType,
            StoreType = FileStoreType.LocalFile,// 默认存储到本地
            Description = description
        };
        if (!await fileService.AddAsync(fileDto))
        {
            return Fail<FileDto>("保存文件失败");
        }

        return Success(fileDto);
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
        if (fileEntity == null || fileEntity.Id < 1)
        {
            return NotFound("文件不存在");
        }
        if (fileEntity.StoreType != FileStoreType.LocalFile)
        {
            return BadRequest("仅支持下载本地文件");
        }
        var filePath = fileEntity.Path;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return NotFound("文件不存在");
        }
        if (filePath.Contains(RootPath))
        {
            filePath = filePath.Replace(RootPath, environment.ContentRootPath);
        }
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("文件不存在");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, fileEntity.ContentType, fileEntity.Name);
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
        if (fileEntity == null || fileEntity.Id < 1)
        {
            return Fail<bool>("文件不存在");
        }

        var filePath = fileEntity.Path;
        if (fileEntity.StoreType == FileStoreType.LocalFile && !string.IsNullOrWhiteSpace(filePath))
        {
            if (filePath.Contains(RootPath))
            {
                filePath = filePath.Replace(RootPath, environment.ContentRootPath);
            }
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        return Success(await fileService.DeleteByIdAsync(id));
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
        return Success(mapper.Map<FileDto>(await fileService.GetByIdAsync(id)));
    }
}