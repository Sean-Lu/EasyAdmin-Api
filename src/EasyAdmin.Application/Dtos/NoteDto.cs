using EasyAdmin.Infrastructure.Enums;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 笔记DTO
/// </summary>
public class NoteDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 分类ID
    /// </summary>
    public long CategoryId { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }
    /// <summary>
    /// 正文格式
    /// </summary>
    public NoteContentType ContentType { get; set; }
    /// <summary>
    /// Markdown正文
    /// </summary>
    public string? ContentMarkdown { get; set; }
    /// <summary>
    /// 富文本内容
    /// </summary>
    public string? ContentHtml { get; set; }
    /// <summary>
    /// 纯文本内容
    /// </summary>
    public string? ContentText { get; set; }
    /// <summary>
    /// 摘要
    /// </summary>
    public string? Summary { get; set; }
    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsTop { get; set; }
    /// <summary>
    /// 是否保护
    /// </summary>
    public bool IsProtected { get; set; }
    /// <summary>
    /// 分类名称
    /// </summary>
    public string? CategoryName { get; set; }
    /// <summary>
    /// 标签
    /// </summary>
    public List<NoteTagDto> Tags { get; set; } = new();
}

/// <summary>
/// 笔记分页请求DTO
/// </summary>
public class NotePageReqDto : PageRequestBase
{
    /// <summary>
    /// 关键词
    /// </summary>
    public string? Keyword { get; set; }
    /// <summary>
    /// 分类ID
    /// </summary>
    public long? CategoryId { get; set; }
    /// <summary>
    /// 标签ID
    /// </summary>
    public List<long>? TagIds { get; set; }
    /// <summary>
    /// 是否保护
    /// </summary>
    public bool? IsProtected { get; set; }
    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool? IsTop { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// 笔记更新DTO
/// </summary>
public class NoteUpdateDto : DtoIdBase
{
    /// <summary>
    /// 分类ID
    /// </summary>
    public long CategoryId { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }
    /// <summary>
    /// 正文格式
    /// </summary>
    public NoteContentType ContentType { get; set; }
    /// <summary>
    /// Markdown正文
    /// </summary>
    public string? ContentMarkdown { get; set; }
    /// <summary>
    /// 富文本内容
    /// </summary>
    public string? ContentHtml { get; set; }
    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsTop { get; set; }
    /// <summary>
    /// 是否保护
    /// </summary>
    public bool IsProtected { get; set; }
    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// 笔记导出请求DTO
/// </summary>
public class NoteExportReqDto
{
    /// <summary>
    /// 笔记ID
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 导出类型
    /// </summary>
    public string ExportType { get; set; }
    /// <summary>
    /// 解锁令牌
    /// </summary>
    public string? UnlockToken { get; set; }
}

/// <summary>
/// Markdown导出请求DTO
/// </summary>
public class NoteMarkdownExportReqDto
{
    /// <summary>
    /// 笔记ID
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 解锁令牌
    /// </summary>
    public string? UnlockToken { get; set; }
}

/// <summary>
/// 笔记批量导出请求DTO
/// </summary>
public class NoteBatchExportReqDto
{
    /// <summary>
    /// 笔记ID集合
    /// </summary>
    public List<long> Ids { get; set; } = new();
    /// <summary>
    /// 导出类型
    /// </summary>
    public string ExportType { get; set; } = string.Empty;
    /// <summary>
    /// 解锁令牌
    /// </summary>
    public string? UnlockToken { get; set; }
}
