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
    /// 用户角色
    /// </summary>
    public virtual UserRole Role { get; set; }
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public virtual DateTime? LastLoginTime { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}