using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 定时任务执行日志表
/// </summary>
[CodeFirst]
public class ScheduleJobLogEntity : TenantEntityBase
{
    /// <summary>
    /// 任务ID
    /// </summary>
    [Description("任务ID")]
    public virtual long JobId { get; set; }
    /// <summary>
    /// 任务名称
    /// </summary>
    [MaxLength(100)]
    [Description("任务名称")]
    public virtual string JobName { get; set; }
    /// <summary>
    /// 任务组
    /// </summary>
    [MaxLength(100)]
    [Description("任务组")]
    public virtual string JobGroup { get; set; }
    /// <summary>
    /// 执行开始时间
    /// </summary>
    [Description("执行开始时间")]
    public virtual DateTime ExecuteStartTime { get; set; }
    /// <summary>
    /// 执行结束时间
    /// </summary>
    [Description("执行结束时间")]
    public virtual DateTime? ExecuteEndTime { get; set; }
    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    [Description("执行耗时（毫秒）")]
    public virtual long? ExecuteElapsedTime { get; set; }
    /// <summary>
    /// 执行状态（0-失败，1-成功）
    /// </summary>
    [Description("执行状态")]
    public virtual int ExecuteStatus { get; set; }
    /// <summary>
    /// 执行结果信息
    /// </summary>
    [Description("执行结果信息")]
    public virtual string? ExecuteMessage { get; set; }
}
