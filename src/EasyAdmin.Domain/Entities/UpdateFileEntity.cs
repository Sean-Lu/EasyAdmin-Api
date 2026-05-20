using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 更新文件实体：记录某个版本包含的全部文件
/// - FilePath 是程序安装目录下的相对路径（如 bin/Core.dll）
/// - Checksum 是文件内容的SHA256（16进制小写64字符），用于版本对比和下载完整性校验
/// - StoragePath 是文件在存储系统中的实际路径（本地文件存储或OSS）
/// </summary>
[CodeFirst]
public class UpdateFileEntity : EntityBase
{
    /// <summary>
    /// 所属版本ID（外键关联 UpdateVersionEntity.Id）
    /// </summary>
    public virtual long VersionId { get; set; }

    /// <summary>
    /// 文件在程序目录下的相对路径（如 bin/Core.dll，支持子目录）
    /// </summary>
    [Required]
    [MaxLength(500)]
    public virtual string FilePath { get; set; }

    /// <summary>
    /// 文件字节大小
    /// </summary>
    public virtual long FileSize { get; set; }

    /// <summary>
    /// 文件SHA256校验和（16进制小写，64字符），用于增量对比和完整性校验
    /// </summary>
    [Required]
    [MaxLength(128)]
    public virtual string Checksum { get; set; }

    /// <summary>
    /// 文件在存储系统中的实际路径
    /// </summary>
    [MaxLength(500)]
    public virtual string StoragePath { get; set; }
}