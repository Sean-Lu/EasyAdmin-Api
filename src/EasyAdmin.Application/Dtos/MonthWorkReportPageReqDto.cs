namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 月报列表分页查询条件
/// </summary>
public class MonthWorkReportPageReqDto : PageRequestBase
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
