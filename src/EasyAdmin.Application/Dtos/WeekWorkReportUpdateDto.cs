using AutoMapper;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(WeekWorkReportEntity), ReverseMap = true)]
public class WeekWorkReportUpdateDto : DtoIdBase
{
    public long UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string WeekWork { get; set; }
    public string? NextWeekPlan { get; set; }
}