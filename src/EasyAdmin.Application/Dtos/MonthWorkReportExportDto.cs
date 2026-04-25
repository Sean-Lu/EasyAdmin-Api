using System.ComponentModel;
using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Attributes;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 月报导出 DTO
/// </summary>
[AutoMap(typeof(MonthWorkReportEntity))]
public class MonthWorkReportExportDto
{
    /// <summary>
    /// 月开始日期
    /// </summary>
    [Description("月开始日期")]
    [DateTimeFormat("yyyy-MM-dd")]
    public virtual DateTime StartTime { get; set; }
    /// <summary>
    /// 月结束日期
    /// </summary>
    [Description("月结束日期")]
    [DateTimeFormat("yyyy-MM-dd")]
    public virtual DateTime EndTime { get; set; }
    /// <summary>
    /// 本月工作
    /// </summary>
    [Description("本月工作")]
    public virtual string MonthWork { get; set; }
    /// <summary>
    /// 下月计划
    /// </summary>
    [Description("下月计划")]
    public virtual string? NextMonthPlan { get; set; }
}
