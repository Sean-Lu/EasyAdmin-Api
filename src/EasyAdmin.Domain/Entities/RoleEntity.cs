using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 角色表
/// </summary>
[CodeFirst]
public class RoleEntity : TenantEntityBase
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Name { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    [MaxLength(50)]
    public virtual string Code { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    [MaxLength(200)]
    public virtual string? Description { get; set; }
    
    /// <summary>
    /// 排序
    /// </summary>
    public virtual int Sort { get; set; }
    
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}