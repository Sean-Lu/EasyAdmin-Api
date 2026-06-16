using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 用户消息 DTO
/// </summary>
public class UserMessageDto : TenantDtoBase
{
    public virtual long NotificationId { get; set; }
    public virtual long UserId { get; set; }
    public virtual string? Title { get; set; }
    public virtual string? Content { get; set; }
    public virtual NotificationNoticeType NoticeType { get; set; }
    public virtual bool IsRead { get; set; }
    public virtual DateTime? ReadTime { get; set; }
    public virtual DateTime? SendTime { get; set; }
}
