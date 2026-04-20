using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 用户表
/// </summary>
//[Table("User")]
[CodeFirst]
[LeftJoin(typeof(DepartmentEntity), nameof(DepartmentId), nameof(DepartmentEntity.Id), "dept")]
[LeftJoin(typeof(PositionEntity), nameof(PositionId), nameof(PositionEntity.Id), "post")]
public class UserEntity : TenantEntityBase
{
    /// <summary>
    /// 用户名称
    /// </summary>
    [MaxLength(50)]
    [Required]
    public virtual string UserName { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    [MaxLength(50)]
    public virtual string Password { get; set; }
    /// <summary>
    /// 昵称
    /// </summary>
    [MaxLength(50)]
    public virtual string NickName { get; set; }
    /// <summary>
    /// 手机号码
    /// </summary>
    [MaxLength(20)]
    public virtual string PhoneNumber { get; set; }
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [MaxLength(50)]
    public virtual string Email { get; set; }
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public virtual DateTime? LastLoginTime { get; set; }
    /// <summary>
    /// 所属部门ID
    /// </summary>
    public virtual long? DepartmentId { get; set; }
    /// <summary>
    /// 所属部门名称
    /// </summary>
    [NotMapped]
    [LeftJoinField("dept", nameof(DepartmentEntity.Name))]
    public virtual string? DepartmentName { get; set; }
    /// <summary>
    /// 所属岗位ID
    /// </summary>
    public virtual long? PositionId { get; set; }
    /// <summary>
    /// 所属岗位名称
    /// </summary>
    [NotMapped]
    [LeftJoinField("post", nameof(PositionEntity.Name))]
    public virtual string? PositionName { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}