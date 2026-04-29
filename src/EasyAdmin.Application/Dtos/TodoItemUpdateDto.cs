using AutoMapper;
using EasyAdmin.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(TodoItemEntity), ReverseMap = true)]
public class TodoItemUpdateDto : DtoIdBase
{
    public long CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public bool Done { get; set; }

    public int Priority { get; set; }

    public int SortOrder { get; set; }
}