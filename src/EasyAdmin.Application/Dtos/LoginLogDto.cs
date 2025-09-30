using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 登录日志表
/// </summary>
[AutoMap(typeof(LoginLogEntity), ReverseMap = true)]
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
    /// 登录时间
    /// </summary>
    public virtual DateTime? LoginTime { get; set; }
    /// <summary>
    /// IP地址
    /// </summary>
    public virtual string? IP { get; set; }
}