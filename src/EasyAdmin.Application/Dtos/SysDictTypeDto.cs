using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 字典类型 DTO
/// </summary>
public class SysDictTypeDto : DtoBase
{
    public virtual string Name { get; set; }
    public virtual string Code { get; set; }
    public virtual string? Remark { get; set; }
    public virtual int Sort { get; set; }
    public virtual CommonState State { get; set; }
}