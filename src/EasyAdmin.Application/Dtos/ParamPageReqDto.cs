namespace EasyAdmin.Application.Dtos;

public class ParamPageReqDto : PageRequestBase
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public string? ParamName { get; set; }
    /// <summary>
    /// 参数键名
    /// </summary>
    public string? ParamKey { get; set; }
    /// <summary>
    /// 参数键值
    /// </summary>
    public string? ParamValue { get; set; }
}