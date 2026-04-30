namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 岗位列表分页查询条件
/// </summary>
public class PositionPageReqDto : PageRequestBase
{
    /// <summary>
    /// 岗位名称
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 岗位编码
    /// </summary>
    public string? Code { get; set; }
}