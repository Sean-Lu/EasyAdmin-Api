using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 签到日志表
/// </summary>
//[Table("CheckInLog")]
[CodeFirst]
public class CheckInLogEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public virtual long UserId { get; set; }
    /// <summary>
    /// 签到类型
    /// </summary>
    [Required]
    public virtual CheckInType CheckInType { get; set; }
    /// <summary>
    /// 签到时间
    /// </summary>
    [Required]
    public virtual DateTime CheckInTime { get; set; }
    /// <summary>
    /// IP地址
    /// </summary>
    [MaxLength(50)]
    public virtual string? IP { get; set; }
}