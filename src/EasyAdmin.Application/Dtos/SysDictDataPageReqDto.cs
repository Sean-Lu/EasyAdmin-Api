namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 字典数据分页查询
/// </summary>
public class SysDictDataPageReqDto : PageRequestBase
{
    public long? DictTypeId { get; set; }
    public string? DictValue { get; set; }
}