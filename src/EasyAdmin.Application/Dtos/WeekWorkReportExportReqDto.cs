namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 周报导出请求参数
/// </summary>
public class WeekWorkReportExportReqDto : WeekWorkReportPageReqDto
{
    /// <summary>
    /// "excel" or "markdown"
    /// </summary>
    public string ExportType { get; set; }
    public bool ExportAll { get; set; }
}
