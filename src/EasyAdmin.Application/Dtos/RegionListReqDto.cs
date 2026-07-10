namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 行政区划列表查询条件
/// </summary>
public class RegionListReqDto
{
    /// <summary>
    /// 是否查询所有行政区划（包括禁用的）
    /// </summary>
    public bool All { get; set; }
    /// <summary>
    /// 行政区划名称
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 层级（1-省，2-市，3-区）
    /// </summary>
    public int? Level { get; set; }
}
