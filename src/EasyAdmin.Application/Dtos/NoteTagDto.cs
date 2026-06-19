using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 笔记标签DTO
/// </summary>
public class NoteTagDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 标签名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    /// <summary>
    /// 使用次数
    /// </summary>
    public int UseCount { get; set; }
}
