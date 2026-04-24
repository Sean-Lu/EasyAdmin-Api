using AutoMapper;
using EasyAdmin.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 待办事项分类DTO
/// </summary>
[AutoMap(typeof(TodoCategoryEntity), ReverseMap = true)]
public class TodoCategoryDto : TenantDtoBase
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
}