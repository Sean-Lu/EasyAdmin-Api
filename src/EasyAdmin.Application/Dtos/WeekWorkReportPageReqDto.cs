namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 周报列表分页查询条件
/// </summary>
public class WeekWorkReportPageReqDto : PageRequestBase
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
