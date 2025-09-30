using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 日报
/// </summary>
[AutoMap(typeof(DayWorkReportEntity), ReverseMap = true)]
public class DayWorkReportDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 日期
    /// </summary>
    public virtual DateTime RecordTime { get; set; }
    /// <summary>
    /// 今日工作
    /// </summary>
    public virtual string TodayWork { get; set; }
    /// <summary>
    /// 明日计划
    /// </summary>
    public virtual string? TomorrowPlan { get; set; }
}