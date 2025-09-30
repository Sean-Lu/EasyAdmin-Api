using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 登录日志表
/// </summary>
//[Table("LoginLog")]
[CodeFirst]
[LeftJoin(typeof(UserEntity), nameof(UserId), nameof(UserEntity.Id), "u")]
public class LoginLogEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public virtual long UserId { get; set; }
    /// <summary>
    /// 用户昵称
    /// </summary>
    [NotMapped]
    [LeftJoinField("u", nameof(UserEntity.NickName))]
    public virtual string UserNickName { get; set; }
    /// <summary>
    /// 登录时间
    /// </summary>
    public virtual DateTime? LoginTime { get; set; }
    /// <summary>
    /// IP地址
    /// </summary>
    [MaxLength(50)]
    public virtual string? IP { get; set; }
}