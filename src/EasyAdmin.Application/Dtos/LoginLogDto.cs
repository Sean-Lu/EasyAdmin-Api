using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 登录日志 DTO
/// </summary>
public class LoginLogDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 用户昵称
    /// </summary>
    public virtual string UserNickName { get; set; }
    /// <summary>
    /// 登录方式
    /// </summary>
    public virtual LoginType LoginType { get; set; } = LoginType.Password;
    /// <summary>
    /// 登录时间
    /// </summary>
    public virtual DateTime? LoginTime { get; set; }
    /// <summary>
    /// IP地址
    /// </summary>
    public virtual string? IP { get; set; }
}