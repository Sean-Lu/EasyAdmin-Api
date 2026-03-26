using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 部门表
/// </summary>
[CodeFirst]
[LeftJoin(typeof(UserEntity), nameof(LeaderId), nameof(UserEntity.Id), "u")]
public class DepartmentEntity : TenantTreeEntityBase<DepartmentEntity>
{
    /// <summary>
    /// 部门名称
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 负责人ID
    /// </summary>
    [MaxLength(50)]
    public virtual long? LeaderId { get; set; }
    /// <summary>
    /// 负责人名称
    /// </summary>
    [NotMapped]
    [LeftJoinField("u", nameof(UserEntity.NickName))]
    public virtual string? LeaderName { get; set; }
    /// <summary>
    /// 联系电话
    /// </summary>
    [MaxLength(20)]
    public virtual string? Phone { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(200)]
    public virtual string? Remark { get; set; }
    /// <summary>
    /// 上级部门完整路径
    /// </summary>
    [NotMapped]
    public virtual string? ParentFullPath { get; set; }
}
