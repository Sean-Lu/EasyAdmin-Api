namespace EasyAdmin.Application.Dtos;

public class DayWorkReportUpdateDto : DtoIdBase
{
    public long UserId { get; set; }
    public DateTime RecordTime { get; set; }
    public string TodayWork { get; set; }
    public string? TomorrowPlan { get; set; }
}