using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 月报表
/// </summary>
[CodeFirst]
public class MonthWorkReportEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 月开始日期
    /// </summary>
    [Required]
    public virtual DateTime StartTime { get; set; }
    /// <summary>
    /// 月结束日期
    /// </summary>
    [Required]
    public virtual DateTime EndTime { get; set; }
    /// <summary>
    /// 本月工作
    /// </summary>
    [MaxLength(2000)]
    public virtual string MonthWork { get; set; }
    /// <summary>
    /// 下月计划
    /// </summary>
    [MaxLength(2000)]
    public virtual string? NextMonthPlan { get; set; }
}
