using System.ComponentModel;
using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Attributes;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 周报导出 DTO
/// </summary>
[AutoMap(typeof(WeekWorkReportEntity))]
public class WeekWorkReportExportDto
{
    /// <summary>
    /// 周开始日期
    /// </summary>
    [Description("周开始日期")]
    [DateTimeFormat("yyyy-MM-dd")]
    public virtual DateTime StartTime { get; set; }
    /// <summary>
    /// 周结束日期
    /// </summary>
    [Description("周结束日期")]
    [DateTimeFormat("yyyy-MM-dd")]
    public virtual DateTime EndTime { get; set; }
    /// <summary>
    /// 本周工作
    /// </summary>
    [Description("本周工作")]
    public virtual string WeekWork { get; set; }
    /// <summary>
    /// 下周计划
    /// </summary>
    [Description("下周计划")]
    public virtual string? NextWeekPlan { get; set; }
}
