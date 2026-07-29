using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 用户消息表
/// </summary>
[CodeFirst]
[LeftJoin(typeof(NotificationEntity), nameof(NotificationId), nameof(NotificationEntity.Id), "n")]
public class UserNotificationEntity : TenantEntityBase
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public virtual long NotificationId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public virtual bool IsRead { get; set; }

    /// <summary>
    /// 已读时间
    /// </summary>
    public virtual DateTime? ReadTime { get; set; }

    /// <summary>
    /// 通知标题
    /// </summary>
    [NotMapped]
    [LeftJoinField("n", nameof(NotificationEntity.Title))]
    public virtual string? Title { get; set; }

    /// <summary>
    /// 通知内容
    /// </summary>
    [NotMapped]
    [LeftJoinField("n", nameof(NotificationEntity.Content))]
    public virtual string? Content { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    [NotMapped]
    [LeftJoinField("n", nameof(NotificationEntity.NoticeType))]
    public virtual NotificationNoticeType NoticeType { get; set; }

    /// <summary>
    /// 目标摘要
    /// </summary>
    [NotMapped]
    [LeftJoinField("n", nameof(NotificationEntity.TargetSummary))]
    public virtual string? TargetSummary { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    [NotMapped]
    [LeftJoinField("n", nameof(NotificationEntity.SendTime))]
    public virtual DateTime? SendTime { get; set; }

    /// <summary>
    /// 通知状态
    /// </summary>
    [NotMapped]
    [LeftJoinField("n", nameof(NotificationEntity.State))]
    public virtual CommonState NoticeState { get; set; }
}
