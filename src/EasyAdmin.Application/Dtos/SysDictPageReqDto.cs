namespace EasyAdmin.Application.Dtos;

public class SysDictPageReqDto : PageRequestBase
{
    /// <summary>
    /// 编码
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// 字典值
    /// </summary>
    public string? DictValue { get; set; }
}