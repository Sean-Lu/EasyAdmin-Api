using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(PositionEntity), ReverseMap = true)]
public class PositionDto : TenantDtoBase
{
    /// <summary>
    /// 岗位名称
    /// </summary>
    public virtual string Name { get; set; }
    /// <summary>
    /// 岗位编码
    /// </summary>
    public virtual string Code { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public virtual int Sort { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
}
