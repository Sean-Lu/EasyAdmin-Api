using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 月报 DTO
/// </summary>
[AutoMap(typeof(MonthWorkReportEntity), ReverseMap = true)]
public class MonthWorkReportDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 月开始日期
    /// </summary>
    public virtual DateTime StartTime { get; set; }
    /// <summary>
    /// 月结束日期
    /// </summary>
    public virtual DateTime EndTime { get; set; }
    /// <summary>
    /// 本月工作
    /// </summary>
    public virtual string MonthWork { get; set; }
    /// <summary>
    /// 下月计划
    /// </summary>
    public virtual string? NextMonthPlan { get; set; }
}
