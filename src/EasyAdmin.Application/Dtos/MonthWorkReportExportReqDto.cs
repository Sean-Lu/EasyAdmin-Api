namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 月报导出请求参数
/// </summary>
public class MonthWorkReportExportReqDto : MonthWorkReportPageReqDto
{
    /// <summary>
    /// "excel" or "markdown"
    /// </summary>
    public string ExportType { get; set; }
    public bool ExportAll { get; set; }
}
