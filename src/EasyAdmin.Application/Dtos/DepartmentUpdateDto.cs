using EasyAdmin.Domain.Contracts;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class DepartmentUpdateDto : DtoIdBase, ITreeIdBase
{
    public long PId { get; set; }
    public int Sort { get; set; }
    public string Name { get; set; }
    public long? LeaderId { get; set; }
    public string? Phone { get; set; }
    public CommonState State { get; set; }
    public string? Remark { get; set; }
}