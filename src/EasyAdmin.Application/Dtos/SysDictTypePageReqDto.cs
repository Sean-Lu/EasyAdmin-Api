namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 字典类型分页查询
/// </summary>
public class SysDictTypePageReqDto : PageRequestBase
{
    public string? Name { get; set; }
    public string? Code { get; set; }
}