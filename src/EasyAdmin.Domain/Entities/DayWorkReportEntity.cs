using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 日报
/// </summary>
//[Table("DayWorkReport")]
[CodeFirst]
public class DayWorkReportEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 日期
    /// </summary>
    [Required]
    public virtual DateTime RecordTime { get; set; }
    /// <summary>
    /// 今日工作
    /// </summary>
    [MaxLength(2000)]
    public virtual string TodayWork { get; set; }
    /// <summary>
    /// 明日计划
    /// </summary>
    [MaxLength(2000)]
    public virtual string? TomorrowPlan { get; set; }
}