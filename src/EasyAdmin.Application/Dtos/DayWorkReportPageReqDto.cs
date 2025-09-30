namespace EasyAdmin.Application.Dtos;

public class DayWorkReportPageReqDto : PageRequestBase
{
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndTime { get; set; }
}

public class DayWorkReportExportReqDto : DayWorkReportPageReqDto
{
    /// <summary>
    /// "excel" or "markdown"
    /// </summary>
    public string ExportType { get; set; }
    public bool ExportAll { get; set; }
}