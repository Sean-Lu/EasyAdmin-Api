using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 笔记保护密码表
/// </summary>
[CodeFirst]
public class UserNotePasswordEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 密码哈希
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string PasswordHash { get; set; }
    /// <summary>
    /// 密码盐
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string PasswordSalt { get; set; }
}
