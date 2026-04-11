namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 登录日志列表分页查询条件
/// </summary>
public class LoginLogPageReqDto : PageRequestBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long? UserId { get; set; }
    /// <summary>
    /// IP地址
    /// </summary>
    public string? IP { get; set; }
}