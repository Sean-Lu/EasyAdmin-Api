namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 日报导出请求参数
/// </summary>
public class DayWorkReportExportReqDto : DayWorkReportPageReqDto
{
    /// <summary>
    /// "excel" or "markdown"
    /// </summary>
    public string ExportType { get; set; }
    public bool ExportAll { get; set; }
}