using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 笔记分类DTO
/// </summary>
public class NoteCategoryDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 分类名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; }
    /// <summary>
    /// 笔记数量
    /// </summary>
    public int NoteCount { get; set; }
}

/// <summary>
/// 笔记分类更新DTO
/// </summary>
public class NoteCategoryUpdateDto : DtoIdBase
{
    /// <summary>
    /// 分类名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; }
}
