using Sean.Core.DbRepository;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 笔记标签表
/// </summary>
[CodeFirst]
public class NoteTagEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 标签名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 使用次数
    /// </summary>
    [DefaultValue(0)]
    public virtual int UseCount { get; set; }
}
