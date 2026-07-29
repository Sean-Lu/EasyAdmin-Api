using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

public class TodoItemUpdateDto : DtoIdBase
{
    public long CategoryId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Name { get; set; }

    public bool Done { get; set; }

    public int Priority { get; set; }

    public int SortOrder { get; set; }
}