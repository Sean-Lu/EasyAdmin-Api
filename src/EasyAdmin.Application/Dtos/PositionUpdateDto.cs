using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class PositionUpdateDto : DtoIdBase
{
    public string Name { get; set; }
    public string Code { get; set; }
    public int Sort { get; set; }
    public CommonState State { get; set; }
    public string? Remark { get; set; }
}