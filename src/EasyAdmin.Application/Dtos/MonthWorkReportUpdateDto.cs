namespace EasyAdmin.Application.Dtos;

public class MonthWorkReportUpdateDto : DtoIdBase
{
    public long UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string MonthWork { get; set; }
    public string? NextMonthPlan { get; set; }
}