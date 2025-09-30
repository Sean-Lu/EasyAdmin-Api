using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 签到统计表
/// </summary>
[AutoMap(typeof(CheckInCountEntity), ReverseMap = true)]
public class CheckInCountDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; }
    /// <summary>
    /// 签到类型
    /// </summary>
    public CheckInType CheckInType { get; set; }
    /// <summary>
    /// 最后签到日期
    /// </summary>
    public DateTime LastCheckInTime { get; set; }
    /// <summary>
    /// 连续签到天数
    /// </summary>
    public int ContinuousCheckInDays { get; set; }
}