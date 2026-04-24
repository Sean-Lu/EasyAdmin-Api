using AutoMapper;
using EasyAdmin.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 待办事项DTO
/// </summary>
[AutoMap(typeof(TodoItemEntity), ReverseMap = true)]
public class TodoItemDto : TenantDtoBase
{
    /// <summary>
    /// 分类ID
    /// </summary>
    public long CategoryId { get; set; }
    /// <summary>
    /// 待办事项名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    /// <summary>
    /// 是否完成
    /// </summary>
    public bool Done { get; set; }
    /// <summary>
    /// 优先级（1-低，2-中，3-高）
    /// </summary>
    public int Priority { get; set; }
    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; }
}