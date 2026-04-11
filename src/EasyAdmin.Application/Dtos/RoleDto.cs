using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 角色 DTO
/// </summary>
[AutoMap(typeof(RoleEntity), ReverseMap = true)]
public class RoleDto : TenantDtoBase
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public virtual string Name { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public virtual string Code { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public virtual string? Description { get; set; }
    
    /// <summary>
    /// 排序
    /// </summary>
    public virtual int Sort { get; set; }
    
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
}