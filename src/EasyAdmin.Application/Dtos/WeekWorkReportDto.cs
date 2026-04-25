using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 周报 DTO
/// </summary>
[AutoMap(typeof(WeekWorkReportEntity), ReverseMap = true)]
public class WeekWorkReportDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 周开始日期
    /// </summary>
    public virtual DateTime StartTime { get; set; }
    /// <summary>
    /// 周结束日期
    /// </summary>
    public virtual DateTime EndTime { get; set; }
    /// <summary>
    /// 本周工作
    /// </summary>
    public virtual string WeekWork { get; set; }
    /// <summary>
    /// 下周计划
    /// </summary>
    public virtual string? NextWeekPlan { get; set; }
}
