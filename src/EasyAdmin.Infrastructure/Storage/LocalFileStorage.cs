using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace EasyAdmin.Infrastructure.Storage;

/// <summary>
/// 本地文件存储
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly ILogger<LocalFileStorage> _logger;
    private readonly string _rootPath;
    private const string RootPathPlaceholder = "${RootPath}";
    private const string DefaultRootPath = ".";

    public LocalFileStorage(ILogger<LocalFileStorage> logger, IConfiguration configuration)
    {
        _logger = logger;
        _rootPath = configuration.GetValue<string>("LocalFileStorage:RootPath", DefaultRootPath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var uploadFilePath = Path.Combine(_rootPath, "UploadFiles", TenantContextHolder.TenantId.ToString(), TenantContextHolder.UserId.ToString());
        if (!Directory.Exists(uploadFilePath))
        {
            Directory.CreateDirectory(uploadFilePath);
        }

        var filePath = Path.Combine(uploadFilePath, fileName);
        if (File.Exists(filePath))
        {
            var extension = Path.GetExtension(fileName);
            filePath = Path.Combine(uploadFilePath, $"{Guid.NewGuid()}{extension}");
        }

        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream);

        var relativePath = filePath;
        if (relativePath.StartsWith(_rootPath))
        {
            relativePath = $"{RootPathPlaceholder}{relativePath.Substring(_rootPath.Length)}";
        }

        _logger.LogInformation("文件上传成功: {FilePath}", relativePath);
        return relativePath;
    }

    public Task<Stream> DownloadAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new FileNotFoundException("文件路径为空");
        }

        var actualPath = filePath;
        if (actualPath.Contains(RootPathPlaceholder))
        {
            actualPath = actualPath.Replace(RootPathPlaceholder, _rootPath);
        }

        if (!File.Exists(actualPath))
        {
            throw new FileNotFoundException("文件不存在", filePath);
        }

        var bytes = File.ReadAllBytes(actualPath);
        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }

    public Task<bool> DeleteAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Task.FromResult(false);
            }

            var actualPath = filePath;
            if (actualPath.Contains(RootPathPlaceholder))
            {
                actualPath = actualPath.Replace(RootPathPlaceholder, _rootPath);
            }

            if (File.Exists(actualPath))
            {
                File.Delete(actualPath);
                _logger.LogInformation("文件删除成功: {FilePath}", filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除文件失败: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        if (filePath.StartsWith("http"))
        {
            return filePath;
        }

        return filePath;
    }
}