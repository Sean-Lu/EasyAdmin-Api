using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 周报表
/// </summary>
[CodeFirst]
public class WeekWorkReportEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 周开始日期
    /// </summary>
    [Required]
    public virtual DateTime StartTime { get; set; }
    /// <summary>
    /// 周结束日期
    /// </summary>
    [Required]
    public virtual DateTime EndTime { get; set; }
    /// <summary>
    /// 本周工作
    /// </summary>
    [MaxLength(2000)]
    public virtual string WeekWork { get; set; }
    /// <summary>
    /// 下周计划
    /// </summary>
    [MaxLength(2000)]
    public virtual string? NextWeekPlan { get; set; }
}
