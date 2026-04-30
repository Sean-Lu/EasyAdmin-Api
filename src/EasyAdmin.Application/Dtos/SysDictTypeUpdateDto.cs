using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class SysDictTypeUpdateDto : DtoIdBase
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Remark { get; set; }
    public int Sort { get; set; }
    public CommonState State { get; set; }
}