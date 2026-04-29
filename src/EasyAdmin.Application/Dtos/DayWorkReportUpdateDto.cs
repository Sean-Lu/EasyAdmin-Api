using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(DayWorkReportEntity), ReverseMap = true)]
public class DayWorkReportUpdateDto : DtoIdBase
{
    public long UserId { get; set; }
    public DateTime RecordTime { get; set; }
    public string TodayWork { get; set; }
    public string? TomorrowPlan { get; set; }
}