using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 定时任务执行日志 DTO
/// </summary>
[AutoMap(typeof(ScheduleJobLogEntity), ReverseMap = true)]
public class ScheduleJobLogDto : TenantDtoBase
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public virtual long JobId { get; set; }
    /// <summary>
    /// 任务名称
    /// </summary>
    public virtual string JobName { get; set; }
    /// <summary>
    /// 任务组
    /// </summary>
    public virtual string JobGroup { get; set; }
    /// <summary>
    /// 执行开始时间
    /// </summary>
    public virtual DateTime ExecuteStartTime { get; set; }
    /// <summary>
    /// 执行结束时间
    /// </summary>
    public virtual DateTime? ExecuteEndTime { get; set; }
    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public virtual long? ExecuteElapsedTime { get; set; }
    /// <summary>
    /// 执行状态（0-失败，1-成功）
    /// </summary>
    public virtual int ExecuteStatus { get; set; }
    /// <summary>
    /// 执行结果信息
    /// </summary>
    public virtual string? ExecuteMessage { get; set; }
}
