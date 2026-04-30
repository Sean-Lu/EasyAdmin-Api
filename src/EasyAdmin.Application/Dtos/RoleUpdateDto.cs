using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class RoleUpdateDto : DtoIdBase
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public int Sort { get; set; }
    public CommonState State { get; set; }
}