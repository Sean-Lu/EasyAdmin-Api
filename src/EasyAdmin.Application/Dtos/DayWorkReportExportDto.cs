using System.ComponentModel;
using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Attributes;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 日报
/// </summary>
[AutoMap(typeof(DayWorkReportEntity))]
public class DayWorkReportExportDto
{
    /// <summary>
    /// 日期
    /// </summary>
    [Description("日期")]
    [DateTimeFormat("yyyy-MM-dd")]
    public virtual DateTime RecordTime { get; set; }
    /// <summary>
    /// 今日工作
    /// </summary>
    [Description("今日工作")]
    public virtual string TodayWork { get; set; }
    /// <summary>
    /// 明日计划
    /// </summary>
    [Description("明日计划")]
    public virtual string? TomorrowPlan { get; set; }
}