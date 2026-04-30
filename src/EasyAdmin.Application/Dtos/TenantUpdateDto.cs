using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class TenantUpdateDto : DtoIdBase
{
    public string Name { get; set; }
    public CommonState State { get; set; }
    public string? Remark { get; set; }
}