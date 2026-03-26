using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(DepartmentEntity), ReverseMap = true)]
public class DepartmentDto : TenantTreeDtoBase<DepartmentDto>
{
    /// <summary>
    /// 部门名称
    /// </summary>
    public virtual string Name { get; set; }
    /// <summary>
    /// 负责人ID
    /// </summary>
    public virtual long? LeaderId { get; set; }
    /// <summary>
    /// 负责人名称
    /// </summary>
    public virtual string? LeaderName { get; set; }
    /// <summary>
    /// 联系电话
    /// </summary>
    public virtual string? Phone { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
    /// <summary>
    /// 上级菜单完整路径
    /// </summary>
    public virtual string? ParentFullPath { get; set; }
}
