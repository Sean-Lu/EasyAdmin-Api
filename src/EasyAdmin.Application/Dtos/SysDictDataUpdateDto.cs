using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

public class SysDictDataUpdateDto : DtoIdBase
{
    public long DictTypeId { get; set; }
    public int DictKey { get; set; }
    public string DictValue { get; set; }
    public int Sort { get; set; }
    public CommonState State { get; set; }
}