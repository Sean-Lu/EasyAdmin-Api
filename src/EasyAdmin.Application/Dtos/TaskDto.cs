using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(TaskEntity), ReverseMap = true)]
public class TaskDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 任务名称
    /// </summary>
    public virtual string TaskName { get; set; }
    /// <summary>
    /// 任务类型
    /// </summary>
    public virtual int TaskType { get; set; }
    /// <summary>
    /// 任务奖励
    /// </summary>
    public virtual string? TaskReward { get; set; }
    /// <summary>
    /// 任务开始时间
    /// </summary>
    public virtual DateTime? TaskStartTime { get; set; }
    /// <summary>
    /// 任务结束时间
    /// </summary>
    public virtual DateTime? TaskEndTime { get; set; }
    /// <summary>
    /// 任务规则
    /// </summary>
    public virtual string? TaskRule { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
}