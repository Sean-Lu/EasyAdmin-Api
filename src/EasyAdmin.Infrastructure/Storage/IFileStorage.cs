namespace EasyAdmin.Infrastructure.Storage;

/// <summary>
/// 文件存储接口
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="fileStream">文件流</param>
    /// <param name="relativePath">相对路径</param>
    /// <returns>文件路径</returns>
    Task<string> UploadAsync(Stream fileStream, string relativePath);
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件流</returns>
    /// <exception cref="ArgumentNullException">文件路径为空</exception>
    /// <exception cref="ArgumentException">文件路径格式错误</exception>
    /// <exception cref="InvalidOperationException">文件不存在</exception>
    Task<Stream> DownloadAsync(string filePath);
    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否删除成功</returns>
    /// <exception cref="ArgumentNullException">文件路径为空</exception>
    /// <exception cref="ArgumentException">文件路径格式错误</exception>
    Task<bool> DeleteAsync(string filePath);
    /// <summary>
    /// 获取文件URL
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件URL</returns>
    /// <exception cref="ArgumentNullException">文件路径为空</exception>
    /// <exception cref="ArgumentException">文件路径格式错误</exception>
    /// <exception cref="InvalidOperationException">文件不存在</exception>
    string GetFileUrl(string filePath);
}