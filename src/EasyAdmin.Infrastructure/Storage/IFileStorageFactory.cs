using EasyAdmin.Infrastructure.Enums;

﻿namespace EasyAdmin.Infrastructure.Storage;

/// <summary>
/// 文件存储工厂接口
/// </summary>
public interface IFileStorageFactory
{
    /// <summary>
    /// 获取文件存储
    /// </summary>
    /// <param name="storeType">文件存储类型</param>
    /// <returns>文件存储实现</returns>
    /// <exception cref="NotSupportedException">不支持的文件存储类型</exception>
    /// <exception cref="InvalidOperationException">未找到对应的文件存储实现</exception>
    IFileStorage GetFileStorage(FileStoreType storeType);
}