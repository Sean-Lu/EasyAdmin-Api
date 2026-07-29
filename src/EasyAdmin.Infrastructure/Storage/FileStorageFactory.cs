using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Infrastructure.Storage;

/// <summary>
/// 文件存储工厂
/// </summary>
public class FileStorageFactory(IEnumerable<IFileStorage> fileStorages) : IFileStorageFactory
{
    /// <inheritdoc />
    public IFileStorage GetFileStorage(FileStoreType storeType)
    {
        var storage = storeType switch
        {
            FileStoreType.LocalFile => fileStorages.FirstOrDefault(s => s is LocalFileStorage),
            FileStoreType.AliyunOSS => fileStorages.FirstOrDefault(s => s is AliyunOssStorage),
            _ => throw new NotSupportedException($"不支持的文件存储类型: {storeType}")
        };

        if (storage == null)
        {
            throw new InvalidOperationException($"未找到对应的文件存储实现: {storeType}");
        }

        return storage;
    }
}