using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 租户表
/// </summary>
//[Table("Tenant")]
[CodeFirst]
[LeftJoin(typeof(UserEntity), nameof(AdminUserId), nameof(UserEntity.Id), "u")]
public class TenantEntity : EntityBase
{
    /// <summary>
    /// 租户名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 租管账号ID
    /// </summary>
    [Required]
    public virtual long AdminUserId { get; set; }
    /// <summary>
    /// 租管账号名称
    /// </summary>
    [NotMapped]
    [LeftJoinField("u", nameof(UserEntity.UserName))]
    public virtual string AdminUserName { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(200)]
    public virtual string Remark { get; set; }
}