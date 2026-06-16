namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 用户消息分页查询 DTO
/// </summary>
public class UserMessagePageReqDto : PageRequestBase
{
    public string? Title { get; set; }
    public bool? IsRead { get; set; }
}
