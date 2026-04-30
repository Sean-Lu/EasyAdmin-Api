using EasyAdmin.Domain.Contracts;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class MenuUpdateDto : DtoIdBase, ITreeIdBase
{
    public long PId { get; set; }
    public int Sort { get; set; }
    public string Icon { get; set; }
    public string Title { get; set; }
    public string Path { get; set; }
    public string OutLink { get; set; }
    public CommonState State { get; set; }
}