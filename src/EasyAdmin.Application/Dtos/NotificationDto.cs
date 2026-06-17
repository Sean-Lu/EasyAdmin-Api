using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 通知 DTO
/// </summary>
public class NotificationDto : TenantDtoBase
{
    /// <summary>
    /// 通知标题
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// 通知内容
    /// </summary>
    public virtual string Content { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public virtual NotificationNoticeType NoticeType { get; set; }

    /// <summary>
    /// 是否发送站内消息
    /// </summary>
    public virtual bool SendInSystem { get; set; } = true;

    /// <summary>
    /// 是否发送邮件
    /// </summary>
    public virtual bool SendEmail { get; set; }

    /// <summary>
    /// 是否发送短信
    /// </summary>
    public virtual bool SendSms { get; set; }

    /// <summary>
    /// 是否发送给所有用户
    /// </summary>
    public virtual bool SendToAll { get; set; }

    /// <summary>
    /// 指定接收用户ID列表
    /// </summary>
    public virtual List<long>? UserIds { get; set; }

    /// <summary>
    /// 指定角色ID列表
    /// </summary>
    public virtual List<long>? RoleIds { get; set; }

    /// <summary>
    /// 指定部门ID列表
    /// </summary>
    public virtual List<long>? DepartmentIds { get; set; }

    /// <summary>
    /// 接收范围摘要
    /// </summary>
    public virtual string? TargetSummary { get; set; }

    /// <summary>
    /// 发送人用户ID
    /// </summary>
    public virtual long SenderUserId { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public virtual DateTime? SendTime { get; set; }

    /// <summary>
    /// 通知状态
    /// </summary>
    public virtual CommonState State { get; set; }
}
