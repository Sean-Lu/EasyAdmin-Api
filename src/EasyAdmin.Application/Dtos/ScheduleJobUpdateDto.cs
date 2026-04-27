using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 更新定时任务 DTO（只包含用户可更新的字段）
/// </summary>
[AutoMap(typeof(ScheduleJobEntity), ReverseMap = true)]
public class ScheduleJobUpdateDto : IdBase
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// 任务类型（0-简单调度，1-Cron调度）
    /// </summary>
    public ScheduleType ScheduleType { get; set; }

    /// <summary>
    /// Cron表达式（Cron调度时使用）
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 简单调度执行间隔（简单调度时使用）
    /// </summary>
    public int? SimpleInterval { get; set; }

    /// <summary>
    /// 简单调度执行间隔单位（简单调度时使用）
    /// </summary>
    public SimpleIntervalUnit SimpleIntervalUnit { get; set; }

    /// <summary>
    /// 任务类名（完整命名空间）
    /// </summary>
    public string JobClassName { get; set; }

    /// <summary>
    /// 任务参数（JSON格式）
    /// </summary>
    public string? JobData { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public CommonState State { get; set; }
}
