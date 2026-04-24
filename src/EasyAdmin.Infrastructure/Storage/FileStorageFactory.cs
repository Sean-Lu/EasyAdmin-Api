using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Infrastructure.Storage;

/// <summary>
/// 文件存储工厂
/// </summary>
public class FileStorageFactory : IFileStorageFactory
{
    private readonly IEnumerable<IFileStorage> _fileStorages;

    public FileStorageFactory(IEnumerable<IFileStorage> fileStorages)
    {
        _fileStorages = fileStorages;
    }

    /// <summary>
    /// 获取文件存储
    /// </summary>
    /// <param name="storeType">文件存储类型</param>
    /// <returns>文件存储实现</returns>
    /// <exception cref="NotSupportedException">不支持的文件存储类型</exception>
    /// <exception cref="InvalidOperationException">未找到对应的文件存储实现</exception>
    public IFileStorage GetFileStorage(FileStoreType storeType)
    {
        var storage = storeType switch
        {
            FileStoreType.LocalFile => _fileStorages.FirstOrDefault(s => s is LocalFileStorage),
            FileStoreType.AliyunOSS => _fileStorages.FirstOrDefault(s => s is AliyunOssStorage),
            _ => throw new NotSupportedException($"不支持的文件存储类型: {storeType}")
        };

        if (storage == null)
        {
            throw new InvalidOperationException($"未找到对应的文件存储实现: {storeType}");
        }

        return storage;
    }
}