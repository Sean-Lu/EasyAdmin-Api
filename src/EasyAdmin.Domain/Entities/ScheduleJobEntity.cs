using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 定时任务表
/// </summary>
[CodeFirst]
public class ScheduleJobEntity : TenantEntityBase
{
    /// <summary>
    /// 任务名称
    /// </summary>
    [MaxLength(100)]
    [Description("任务名称")]
    public virtual string JobName { get; set; }
    /// <summary>
    /// 任务类型（0-简单调度，1-Cron调度）
    /// </summary>
    [Description("任务类型")]
    public virtual ScheduleType ScheduleType { get; set; }
    /// <summary>
    /// Cron表达式（Cron调度时使用）
    /// </summary>
    [MaxLength(100)]
    [Description("Cron表达式")]
    public virtual string? CronExpression { get; set; }
    /// <summary>
    /// 简单调度执行间隔（简单调度时使用）
    /// </summary>
    [Description("简单调度执行间隔")]
    public virtual int? SimpleInterval { get; set; }
    /// <summary>
    /// 简单调度执行间隔单位（简单调度时使用）
    /// </summary>
    [Description("简单调度执行间隔单位")]
    public virtual SimpleIntervalUnit SimpleIntervalUnit { get; set; }
    /// <summary>
    /// 任务类名（完整命名空间）
    /// </summary>
    [MaxLength(500)]
    [Description("任务类名")]
    public virtual string JobClassName { get; set; }
    /// <summary>
    /// 任务参数（JSON格式）
    /// </summary>
    [MaxLength(2000)]
    [Description("任务参数")]
    public virtual string? JobData { get; set; }
    /// <summary>
    /// 任务描述
    /// </summary>
    [MaxLength(500)]
    [Description("任务描述")]
    public virtual string? Description { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Disable)]
    [Description("状态")]
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 上次执行时间
    /// </summary>
    [Description("上次执行时间")]
    public virtual DateTime? LastExecuteTime { get; set; }
    /// <summary>
    /// 下次执行时间
    /// </summary>
    [Description("下次执行时间")]
    public virtual DateTime? NextExecuteTime { get; set; }
}
