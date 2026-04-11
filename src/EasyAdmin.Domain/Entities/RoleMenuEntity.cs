using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 角色菜单权限表
/// </summary>
[CodeFirst]
public class RoleMenuEntity : TenantEntityBase
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [Required]
    public virtual long RoleId { get; set; }
    
    /// <summary>
    /// 菜单ID
    /// </summary>
    [Required]
    public virtual long MenuId { get; set; }
}