using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 文件表
/// </summary>
[AutoMap(typeof(FileEntity), ReverseMap = true)]
public class FileDto : TenantDtoBase
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public virtual string Name { get; set; }
    /// <summary>
    /// 文件路径
    /// </summary>
    public virtual string Path { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public virtual long Size { get; set; }
    /// <summary>
    /// 文件类型
    /// </summary>
    public virtual string? ContentType { get; set; }
    /// <summary>
    /// 文件存储类型
    /// </summary>
    public virtual FileStoreType StoreType { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public virtual string? Description { get; set; }
}