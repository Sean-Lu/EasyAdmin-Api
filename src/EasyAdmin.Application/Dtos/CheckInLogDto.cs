using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 签到日志表
/// </summary>
[AutoMap(typeof(CheckInLogEntity), ReverseMap = true)]
public class CheckInLogDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 签到类型
    /// </summary>
    public virtual CheckInType CheckInType { get; set; }
    /// <summary>
    /// 签到时间
    /// </summary>
    public virtual DateTime CheckInTime { get; set; }
    /// <summary>
    /// IP地址
    /// </summary>
    public virtual string? IP { get; set; }
}