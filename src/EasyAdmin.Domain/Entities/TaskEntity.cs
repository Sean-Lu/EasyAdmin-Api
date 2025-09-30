using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 任务表
/// </summary>
//[Table("Task")]
[CodeFirst]
public class TaskEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 任务名称
    /// </summary>
    [MaxLength(50)]
    public virtual string TaskName { get; set; }
    /// <summary>
    /// 任务类型
    /// </summary>
    public virtual int TaskType { get; set; }
    /// <summary>
    /// 任务奖励
    /// </summary>
    [MaxLength(50)]
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
    [MaxLength(200)]
    public virtual string? TaskRule { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}