namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 字典列表分页查询条件
/// </summary>
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