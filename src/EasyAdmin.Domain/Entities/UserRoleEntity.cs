using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 用户角色关联表
/// </summary>
[CodeFirst]
public class UserRoleEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public virtual long UserId { get; set; }
    
    /// <summary>
    /// 角色ID
    /// </summary>
    [Required]
    public virtual long RoleId { get; set; }
}