using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 行政区划 DTO
/// </summary>
public class RegionDto : TreeDtoBase<RegionDto>
{
    /// <summary>
    /// 行政区划名称
    /// </summary>
    public virtual string Name { get; set; }
    /// <summary>
    /// 行政区划代码
    /// </summary>
    public virtual string Code { get; set; }
    /// <summary>
    /// 层级（1-省，2-市，3-区）
    /// </summary>
    public virtual int Level { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 上级行政区划完整路径
    /// </summary>
    public virtual string? ParentFullPath { get; set; }
}
