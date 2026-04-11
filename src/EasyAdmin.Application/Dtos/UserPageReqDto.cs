namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 用户列表分页查询条件
/// </summary>
public class UserPageReqDto : PageRequestBase
{
    /// <summary>
    /// 用户名称
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// 手机号码
    /// </summary>
    public string? PhoneNumber { get; set; }
    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string? Email { get; set; }
}