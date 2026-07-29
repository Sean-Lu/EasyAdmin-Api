using System.IO.Compression;
using System.Security.Cryptography;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Storage;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 版本管理服务实现
/// </summary>
public class VersionService(
    ILogger<VersionService> logger,
    IMapper mapper,
    IConfiguration configuration,
    IUpdateVersionRepository versionRepository,
    IUpdateFileRepository fileRepository,
    IFileStorageFactory fileStorageFactory
) : IVersionService
{
    private const string UpdatePackagesDir = "update-packages";

    /// <summary>上传文件允许的扩展名白名单（防止恶意文件上传）</summary>
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".exe", ".json", ".xml", ".config", ".pdb",
        ".png", ".ico", ".jpg", ".jpeg", ".gif", ".bmp", ".svg",
        ".html", ".htm", ".css", ".js", ".map",
        ".txt", ".md", ".csv", ".log",
        ".ttf", ".woff", ".woff2",
        ".so", ".dylib", ".a"
    };

    /// <inheritdoc />
    public async Task<UpdateCheckResultDto> CheckAsync(int currentVersionCode, string appCode, string platform)
    {
        var orderBy = OrderByConditionBuilder<UpdateVersionEntity>.Build(OrderByType.Desc, entity => entity.VersionCode);
        var latest = (await versionRepository.QueryAsync(
            entity => entity.AppCode == appCode
                      && entity.Platform == platform
                      && entity.State == CommonState.Enable
                      && !entity.IsDelete,
            orderBy, 1))?.FirstOrDefault();

        if (latest == null || latest.VersionCode <= currentVersionCode)
        {
            return new UpdateCheckResultDto { HasUpdate = false };
        }

        // 强制更新条件：手动标记 || 当前版本低于最低支持版本
        var forceUpdate = latest.IsForceUpdate || currentVersionCode < latest.MinSupportedVersionCode;

        return new UpdateCheckResultDto
        {
            HasUpdate = true,
            LatestVersionName = latest.VersionName,
            LatestVersionCode = latest.VersionCode,
            Changelog = latest.Changelog,
            ForceUpdate = forceUpdate,
            PublishTime = latest.PublishTime
        };
    }

    public async Task<UpdateCheckResultDto?> GetLatestAsync(string appCode, string platform)
    {
        var orderBy = OrderByConditionBuilder<UpdateVersionEntity>.Build(OrderByType.Desc, entity => entity.VersionCode);
        var latest = (await versionRepository.QueryAsync(
            entity => entity.AppCode == appCode
                      && entity.Platform == platform
                      && entity.State == CommonState.Enable
                      && !entity.IsDelete,
            orderBy, 1))?.FirstOrDefault();

        if (latest == null)
        {
            return null;
        }

        return new UpdateCheckResultDto
        {
            HasUpdate = false,
            LatestVersionName = latest.VersionName,
            LatestVersionCode = latest.VersionCode,
            PublishTime = latest.PublishTime
        };
    }

    /// <inheritdoc />
    public async Task<UpdateManifestResultDto> GetManifestAsync(int currentVersionCode, int targetVersionCode, string appCode, string platform)
    {
        var orderBy = OrderByConditionBuilder<UpdateVersionEntity>.Build(OrderByType.Desc, entity => entity.VersionCode);
        var targetVersion = (await versionRepository.QueryAsync(
            entity => entity.VersionCode == targetVersionCode
                      && entity.AppCode == appCode
                      && entity.Platform == platform
                      && !entity.IsDelete,
            orderBy, 1))?.FirstOrDefault();

        if (targetVersion == null)
        {
            throw new ExplicitException($"目标版本不存在: {targetVersionCode}");
        }

        // 获取目标版本的全部文件
        var targetFiles = (await fileRepository.QueryAsync(
            entity => entity.VersionId == targetVersion.Id && !entity.IsDelete))?.ToList() ?? new();

        // 获取当前版本的全部文件（首次安装时不查询）
        List<UpdateFileEntity> currentFiles = new();
        if (currentVersionCode > 0)
        {
            var currentVersion = (await versionRepository.QueryAsync(
                entity => entity.VersionCode == currentVersionCode
                          && entity.AppCode == appCode
                          && entity.Platform == platform
                          && !entity.IsDelete,
                orderBy, 1))?.FirstOrDefault();

            if (currentVersion != null)
            {
                currentFiles = (await fileRepository.QueryAsync(
                    entity => entity.VersionId == currentVersion.Id && !entity.IsDelete))?.ToList() ?? new();
            }
        }

        var targetDict = targetFiles.ToDictionary(f => f.FilePath, f => f);
        var currentDict = currentFiles.ToDictionary(f => f.FilePath, f => f);

        var result = new UpdateManifestResultDto
        {
            TargetVersionName = targetVersion.VersionName,
            TargetVersionCode = targetVersion.VersionCode
        };

        // 遍历目标文件：校验和不同的标记为update，新增的标记为add
        foreach (var kv in targetDict)
        {
            if (currentDict.TryGetValue(kv.Key, out var currentFile))
            {
                if (!string.Equals(currentFile.Checksum, kv.Value.Checksum, StringComparison.OrdinalIgnoreCase))
                {
                    result.FilesToDownload.Add(new FileEntryDto
                    {
                        FilePath = kv.Value.FilePath,
                        FileSize = kv.Value.FileSize,
                        Checksum = kv.Value.Checksum,
                        Action = "update"
                    });
                }
            }
            else
            {
                result.FilesToDownload.Add(new FileEntryDto
                {
                    FilePath = kv.Value.FilePath,
                    FileSize = kv.Value.FileSize,
                    Checksum = kv.Value.Checksum,
                    Action = "add"
                });
            }
        }

        // 遍历当前文件：目标版本中不存在的标记为delete
        foreach (var kv in currentDict)
        {
            if (!targetDict.ContainsKey(kv.Key))
            {
                result.FilesToDelete.Add(new FileEntryDto
                {
                    FilePath = kv.Key,
                    FileSize = kv.Value.FileSize,
                    Checksum = kv.Value.Checksum,
                    Action = "delete"
                });
            }
        }

        result.TotalDownloadSize = result.FilesToDownload.Sum(f => f.FileSize);
        return result;
    }

    /// <inheritdoc />
    public async Task<(Stream Stream, string Checksum, string FileName)> GetFileStreamAsync(int versionCode, string filePath, string appCode, string platform)
    {
        var orderBy = OrderByConditionBuilder<UpdateVersionEntity>.Build(OrderByType.Desc, entity => entity.VersionCode);
        var version = (await versionRepository.QueryAsync(
            entity => entity.VersionCode == versionCode
                      && entity.AppCode == appCode
                      && entity.Platform == platform
                      && !entity.IsDelete,
            orderBy, 1))?.FirstOrDefault()
            ?? throw new FileNotFoundException($"版本不存在: {versionCode}");

        var file = (await fileRepository.QueryAsync(
            entity => entity.VersionId == version.Id
                      && entity.FilePath == filePath
                      && !entity.IsDelete))?.FirstOrDefault()
            ?? throw new FileNotFoundException($"文件不存在: {filePath}");

        var storage = fileStorageFactory.GetFileStorage(GetStorageType());
        var stream = await storage.DownloadAsync(file.StoragePath);
        return (stream, file.Checksum, file.FilePath);
    }

    /// <inheritdoc />
    public async Task<(Stream ZipStream, string VersionName)> DownloadZipAsync(long versionId)
    {
        var entity = await versionRepository.GetByIdAsync(versionId);
        if (entity == null || entity.IsDelete)
        {
            throw new ExplicitException("版本不存在");
        }

        var files = (await fileRepository.QueryAsync(
            f => f.VersionId == entity.Id && !f.IsDelete))?.ToList() ?? new();

        var storage = fileStorageFactory.GetFileStorage(GetStorageType());
        var zipStream = new MemoryStream();

        // 动态创建zip包，保持原始目录结构
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.FilePath, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await using var fileStream = await storage.DownloadAsync(file.StoragePath);
                await fileStream.CopyToAsync(entryStream);
            }
        }

        zipStream.Position = 0;
        return (zipStream, $"{entity.VersionName}_{entity.Platform}.zip");
    }

    /// <inheritdoc />
    public async Task<long> RegisterAsync(UpdateVersionAddDto dto, string zipFilePath)
    {
        var maxVersionCode = (await versionRepository.QueryAsync(entity => entity.AppCode == dto.AppCode && entity.Platform == dto.Platform, fieldExpression: entity => entity.VersionCode))?
            .Max(e => (int?)e.VersionCode) ?? 0;
        var versionCode = maxVersionCode + 1;
        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException("上传的zip文件不存在");
        }

        var existing = (await versionRepository.QueryAsync(
            entity => entity.AppCode == dto.AppCode
                      && entity.Platform == dto.Platform
                      && entity.VersionName == dto.VersionName
                      && !entity.IsDelete))?.FirstOrDefault();

        if (existing != null)
        {
            throw new ExplicitException($"版本 {dto.VersionName}({versionCode}) 在平台 {dto.Platform} 下已存在");
        }

        var storage = fileStorageFactory.GetFileStorage(GetStorageType());
        var versionDir = $"{UpdatePackagesDir}/{dto.AppCode}/{dto.Platform}/{versionCode}";

        var versionEntity = new UpdateVersionEntity
        {
            AppCode = dto.AppCode,
            VersionName = dto.VersionName,
            VersionCode = versionCode,
            Platform = dto.Platform,
            Changelog = dto.Changelog ?? string.Empty,
            IsForceUpdate = dto.IsForceUpdate,
            MinSupportedVersionCode = dto.MinSupportedVersionCode,
            State = CommonState.Disable
        };

        await versionRepository.AddAsync(versionEntity);

        try
        {
            // 增量包模式：自动查询同平台最新已发布版本作为继承基准
            Dictionary<string, UpdateFileEntity>? baseFileDict = null;
            if (dto.UpdateType == UpdatePackageType.Incremental)
            {
                var baseVersion = (await versionRepository.QueryAsync(
                    entity => entity.AppCode == dto.AppCode
                              && entity.Platform == dto.Platform
                              && entity.State == CommonState.Enable
                              && !entity.IsDelete))?.FirstOrDefault();

                if (baseVersion != null)
                {
                    var baseFiles = await fileRepository.QueryAsync(
                        entity => entity.VersionId == baseVersion.Id && !entity.IsDelete);
                    baseFileDict = baseFiles?.ToDictionary(f => f.FilePath, f => f)
                                   ?? new Dictionary<string, UpdateFileEntity>();
                }
                else
                {
                    logger.LogWarning("增量包上传但未找到同平台({Platform})的已发布基准版本，将作为全量包处理", dto.Platform);
                }
            }

            // 解压zip到临时目录，遍历所有文件计算SHA256并存储
            var extractDir = Path.Combine(Path.GetTempPath(), $"EasyUpdate_Register_{Guid.NewGuid():N}");
            try
            {
                ZipFile.ExtractToDirectory(zipFilePath, extractDir);

                var allFiles = Directory.GetFiles(extractDir, "*", SearchOption.AllDirectories);
                var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var filePath in allFiles)
                {
                    var relativePath = Path.GetRelativePath(extractDir, filePath)
                        .Replace(Path.DirectorySeparatorChar, '/');

                    // 路径穿越防护：拒绝包含 ".." 或以 "/" 开头的恶意路径
                    if (relativePath.Contains("..") || relativePath.StartsWith('/') || Path.IsPathRooted(relativePath))
                    {
                        logger.LogWarning("检测到可疑路径穿越，已拒绝: {Path}", relativePath);
                        continue;
                    }

                    // 文件类型白名单校验
                    var extension = Path.GetExtension(relativePath);
                    if (!string.IsNullOrEmpty(extension) && !AllowedExtensions.Contains(extension))
                    {
                        logger.LogWarning("文件类型不在白名单中，已拒绝: {Path} (扩展名: {Ext})", relativePath, extension);
                        continue;
                    }

                    var fileSize = new FileInfo(filePath).Length;
                    var checksum = ComputeSha256(filePath);

                    var storagePath = $"{versionDir}/{relativePath}";
                    await using var fileStream = File.OpenRead(filePath);
                    await storage.UploadAsync(fileStream, storagePath);

                    await fileRepository.AddAsync(new UpdateFileEntity
                    {
                        VersionId = versionEntity.Id,
                        FilePath = relativePath,
                        FileSize = fileSize,
                        Checksum = checksum,
                        StoragePath = storagePath
                    });

                    processedFiles.Add(relativePath);
                }

                // 增量包：继承基础版本中未被修改的文件（路径一致的视为未变）
                if (dto.UpdateType == UpdatePackageType.Incremental && baseFileDict != null)
                {
                    foreach (var kv in baseFileDict)
                    {
                        if (!processedFiles.Contains(kv.Key))
                        {
                            await fileRepository.AddAsync(new UpdateFileEntity
                            {
                                VersionId = versionEntity.Id,
                                FilePath = kv.Value.FilePath,
                                FileSize = kv.Value.FileSize,
                                Checksum = kv.Value.Checksum,
                                StoragePath = kv.Value.StoragePath
                            });
                        }
                    }
                }
            }
            finally
            {
                if (Directory.Exists(extractDir))
                {
                    Directory.Delete(extractDir, true);
                }
            }

            logger.LogInformation("版本注册成功: {VersionName}({VersionCode}), 平台: {Platform}, 类型: {UpdateType}",
                dto.VersionName, versionCode, dto.Platform, dto.UpdateType);

            return versionEntity.Id;
        }
        catch (Exception)
        {
            // 注册过程异常：清理已插入的数据，保证原子性
            await fileRepository.DeleteAsync(entity => entity.VersionId == versionEntity.Id);
            await versionRepository.DeleteByIdAsync(versionEntity.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PageQueryResult<UpdateVersionDto>> PageAsync(UpdateVersionPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<UpdateVersionEntity>.Build(OrderByType.Desc, entity => entity.VersionCode);
        orderBy.Next = OrderByConditionBuilder<UpdateVersionEntity>.Build(OrderByType.Desc, entity => entity.Id);

        var pageResult = await versionRepository.PageQueryAsync(
            WhereExpressionUtil.Create<UpdateVersionEntity>(entity => !entity.IsDelete)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.AppCode), entity => entity.AppCode == request.AppCode)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.VersionName), entity => entity.VersionName.Contains(request.VersionName))
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Platform), entity => entity.Platform == request.Platform),
            orderBy, request.PageNumber, request.PageSize);

        var result = new PageQueryResult<UpdateVersionDto>
        {
            Total = pageResult.Total,
            List = new List<UpdateVersionDto>()
        };

        foreach (var entity in pageResult.List)
        {
            var files = (await fileRepository.QueryAsync(
                f => f.VersionId == entity.Id && !f.IsDelete))?.ToList() ?? new List<UpdateFileEntity>();

            var dto = mapper.Map<UpdateVersionDto>(entity);
            dto.FileCount = files.Count;
            dto.TotalSize = files.Sum(f => f.FileSize);
            result.List.Add(dto);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<UpdateVersionDetailDto> DetailAsync(long id)
    {
        var entity = await versionRepository.GetByIdAsync(id);
        if (entity == null || entity.IsDelete)
        {
            throw new ExplicitException("版本不存在");
        }

        var dto = mapper.Map<UpdateVersionDetailDto>(entity);
        var files = (await fileRepository.QueryAsync(
            f => f.VersionId == entity.Id && !f.IsDelete))?.ToList() ?? new();

        dto.Files = files.Select(f => new FileEntryDto
        {
            FilePath = f.FilePath,
            FileSize = f.FileSize,
            Checksum = f.Checksum,
            Action = string.Empty
        }).OrderBy(f => f.FilePath).ToList();

        dto.FileCount = dto.Files.Count;
        dto.TotalSize = files.Sum(f => f.FileSize);

        return dto;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(UpdateVersionUpdateDto dto)
    {
        return await versionRepository.UpdateAsync(new UpdateVersionEntity
        {
            Id = dto.Id,
            VersionName = dto.VersionName,
            Changelog = dto.Changelog,
            IsForceUpdate = dto.IsForceUpdate,
            MinSupportedVersionCode = dto.MinSupportedVersionCode
        }, entity => new { entity.VersionName, entity.Changelog, entity.IsForceUpdate, entity.MinSupportedVersionCode }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        var entity = await versionRepository.GetByIdAsync(id);
        if (entity == null || entity.IsDelete)
        {
            throw new ExplicitException("版本不存在");
        }

        if (state == CommonState.Enable)
        {
            // 发布新版本：先禁用同 (AppCode,Platform) 所有已发布版本，再启用目标版本
            var published = (await versionRepository.QueryAsync(
                v => v.AppCode == entity.AppCode
                     && v.Platform == entity.Platform
                     && v.State == CommonState.Enable
                     && v.Id != id
                     && !v.IsDelete))?.ToList() ?? new();

            foreach (var p in published)
            {
                await versionRepository.UpdateAsync(new UpdateVersionEntity
                {
                    Id = p.Id,
                    State = CommonState.Disable
                }, v => v.State);
            }

            await versionRepository.UpdateAsync(new UpdateVersionEntity
            {
                Id = id,
                State = CommonState.Enable,
                PublishTime = DateTime.Now
            }, v => new { v.State, v.PublishTime });
        }
        else
        {
            await versionRepository.UpdateAsync(new UpdateVersionEntity
            {
                Id = id,
                State = state
            }, v => v.State);
        }

        logger.LogInformation("版本状态变更: Id={Id}, Status={Status}", id, state);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await versionRepository.GetByIdAsync(id);
        if (entity == null || entity.IsDelete)
        {
            throw new ExplicitException("版本不存在");
        }

        if (entity.State == CommonState.Enable)
        {
            throw new ExplicitException("不能删除已发布的版本，请先回滚");
        }

        // 清理文件存储中的物理文件
        var files = await fileRepository.QueryAsync(f => f.VersionId == id && !f.IsDelete);
        if (files != null)
        {
            var storage = fileStorageFactory.GetFileStorage(GetStorageType());
            foreach (var file in files)
            {
                try
                {
                    await storage.DeleteAsync(file.StoragePath);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "删除存储文件失败: {StoragePath}", file.StoragePath);
                }
            }
        }

        await fileRepository.DeleteAsync(f => f.VersionId == id);
        return await versionRepository.DeleteByIdAsync(id);
    }

    /// <summary>
    /// 读取文件存储类型配置，默认使用本地文件存储
    /// </summary>
    private FileStoreType GetStorageType()
    {
        var typeStr = configuration.GetValue<string>("UpdateService:StorageType");
        return Enum.TryParse<FileStoreType>(typeStr, out var type) ? type : FileStoreType.LocalFile;
    }

    /// <summary>
    /// 计算文件的SHA256哈希值（16进制小写字符串，64字符），用于文件完整性校验和版本对比
    /// </summary>
    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
