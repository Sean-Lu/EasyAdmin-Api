using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 笔记表
/// </summary>
[CodeFirst]
public class NoteEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 分类ID
    /// </summary>
    public virtual long CategoryId { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string Title { get; set; }
    /// <summary>
    /// 正文格式
    /// </summary>
    [DefaultValue(NoteContentType.RichText)]
    public virtual NoteContentType ContentType { get; set; }
    /// <summary>
    /// Markdown正文
    /// </summary>
    public virtual string? ContentMarkdown { get; set; }
    /// <summary>
    /// 富文本内容
    /// </summary>
    public virtual string? ContentHtml { get; set; }
    /// <summary>
    /// 纯文本内容
    /// </summary>
    public virtual string? ContentText { get; set; }
    /// <summary>
    /// 摘要
    /// </summary>
    [MaxLength(300)]
    public virtual string? Summary { get; set; }
    /// <summary>
    /// 是否置顶
    /// </summary>
    [DefaultValue(false)]
    public virtual bool IsTop { get; set; }
    /// <summary>
    /// 是否保护
    /// </summary>
    [DefaultValue(false)]
    public virtual bool IsProtected { get; set; }
}
