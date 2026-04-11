using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 用户角色关联 DTO
/// </summary>
[AutoMap(typeof(UserRoleEntity), ReverseMap = true)]
public class UserRoleDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    
    /// <summary>
    /// 角色ID
    /// </summary>
    public virtual long RoleId { get; set; }
}