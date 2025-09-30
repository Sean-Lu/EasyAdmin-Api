using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 文件表
/// </summary>
//[Table("File")]
[CodeFirst]
public class FileEntity : TenantEntityBase
{
    /// <summary>
    /// 文件名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 文件路径
    /// </summary>
    [MaxLength(200)]
    public virtual string Path { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public virtual long Size { get; set; }
    /// <summary>
    /// 文件类型
    /// </summary>
    [MaxLength(100)]
    public virtual string? ContentType { get; set; }
    /// <summary>
    /// 文件存储类型
    /// </summary>
    public virtual FileStoreType StoreType { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(200)]
    public virtual string? Description { get; set; }
}