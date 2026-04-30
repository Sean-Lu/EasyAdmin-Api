using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 参数配置 DTO
/// </summary>
public class ParamDto : DtoBase
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public virtual string ParamName { get; set; }
    /// <summary>
    /// 参数键名
    /// </summary>
    public virtual string ParamKey { get; set; }
    /// <summary>
    /// 参数键值
    /// </summary>
    public virtual string ParamValue { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
}