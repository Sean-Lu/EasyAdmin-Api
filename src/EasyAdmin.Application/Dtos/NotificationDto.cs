using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 通知 DTO
/// </summary>
public class NotificationDto : TenantDtoBase
{
    public virtual string Title { get; set; }
    public virtual string Content { get; set; }
    public virtual NotificationNoticeType NoticeType { get; set; }
    public virtual bool SendToAll { get; set; }
    public virtual List<long>? UserIds { get; set; }
    public virtual List<long>? RoleIds { get; set; }
    public virtual List<long>? DepartmentIds { get; set; }
    public virtual string? TargetSummary { get; set; }
    public virtual long SenderUserId { get; set; }
    public virtual DateTime? SendTime { get; set; }
    public virtual CommonState State { get; set; }
}
