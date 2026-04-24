using Aliyun.OSS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EasyAdmin.Infrastructure.Models;

namespace EasyAdmin.Infrastructure.Storage;

/// <summary>
/// 阿里云OSS文件存储
/// </summary>
public class AliyunOssStorage : IFileStorage
{
    private readonly ILogger<AliyunOssStorage> _logger;
    private readonly AliyunOssConfig _config;
    private readonly OssClient _ossClient;

    public AliyunOssStorage(ILogger<AliyunOssStorage> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = new AliyunOssConfig();
        configuration.GetSection("AliyunOss").Bind(_config);
        _ossClient = new OssClient(_config.Endpoint, _config.AccessKeyId, _config.AccessKeySecret);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var objectName = GenerateObjectName(fileName);
            var putRequest = new PutObjectRequest(_config.BucketName, objectName, fileStream);

            var result = await Task.Run(() => _ossClient.PutObject(putRequest));

            if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("文件上传到OSS成功: {ObjectName}", objectName);
                return objectName;
            }

            throw new InvalidOperationException($"上传失败，状态码: {result.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传到OSS失败: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new FileNotFoundException("文件路径为空");
            }

            var objectName = ExtractObjectName(filePath);
            var result = await Task.Run(() => _ossClient.GetObject(_config.BucketName, objectName));

            if (result != null)
            {
                return result.Content;
            }

            throw new FileNotFoundException("文件不存在");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从OSS下载文件失败: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var objectName = ExtractObjectName(filePath);
            var result = await Task.Run(() => _ossClient.DeleteObject(_config.BucketName, objectName));

            if (result.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
            {
                _logger.LogInformation("文件从OSS删除成功: {ObjectName}", objectName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从OSS删除文件失败: {FilePath}", filePath);
            return false;
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

        if (!string.IsNullOrWhiteSpace(_config.PublicDomain))
        {
            return $"{_config.PublicDomain.TrimEnd('/')}/{filePath}";
        }

        return $"https://{_config.BucketName}.{_config.Endpoint}/{filePath}";
    }

    private static string GenerateObjectName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return $"{DateTime.Now:yyyyMMdd}/{Guid.NewGuid()}{extension}";
    }

    private static string ExtractObjectName(string filePath)
    {
        if (filePath.StartsWith("http"))
        {
            var uri = new Uri(filePath);
            return uri.AbsolutePath.TrimStart('/');
        }

        return filePath;
    }
}