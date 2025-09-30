using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 签到统计表
/// </summary>
//[Table("CheckInCount")]
[CodeFirst]
public class CheckInCountEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public long UserId { get; set; }
    /// <summary>
    /// 签到类型
    /// </summary>
    [Required]
    public CheckInType CheckInType { get; set; }
    /// <summary>
    /// 最后签到日期
    /// </summary>
    public DateTime LastCheckInTime { get; set; }
    /// <summary>
    /// 连续签到天数
    /// </summary>
    public int ContinuousCheckInDays { get; set; }
}