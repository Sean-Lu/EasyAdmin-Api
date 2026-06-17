using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 通知表
/// </summary>
[CodeFirst]
public class NotificationEntity : TenantEntityBase
{
    /// <summary>
    /// 标题
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public virtual string Content { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public virtual NotificationNoticeType NoticeType { get; set; }

    /// <summary>
    /// 是否发送站内消息
    /// </summary>
    [DefaultValue(true)]
    public virtual bool SendInSystem { get; set; }

    /// <summary>
    /// 是否发送邮件
    /// </summary>
    [DefaultValue(false)]
    public virtual bool SendEmail { get; set; }

    /// <summary>
    /// 是否发送短信
    /// </summary>
    [DefaultValue(false)]
    public virtual bool SendSms { get; set; }

    /// <summary>
    /// 接收范围摘要
    /// </summary>
    [MaxLength(500)]
    public virtual string? TargetSummary { get; set; }

    /// <summary>
    /// 发送人ID
    /// </summary>
    public virtual long SenderUserId { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public virtual DateTime? SendTime { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}
