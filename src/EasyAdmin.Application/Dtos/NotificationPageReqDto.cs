using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 通知分页查询 DTO
/// </summary>
public class NotificationPageReqDto : PageRequestBase
{
    public string? Title { get; set; }
    public NotificationNoticeType? NoticeType { get; set; }
}
