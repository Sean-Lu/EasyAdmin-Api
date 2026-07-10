using EasyAdmin.Domain.Contracts;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class RegionUpdateDto : DtoIdBase, ITreeIdBase
{
    public long PId { get; set; }
    public int Sort { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public int Level { get; set; }
    public CommonState State { get; set; }
}
