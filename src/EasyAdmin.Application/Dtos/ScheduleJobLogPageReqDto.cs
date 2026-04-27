namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 定时任务执行日志分页查询请求 DTO
/// </summary>
public class ScheduleJobLogPageReqDto : PageRequestBase
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public long? JobId { get; set; }
    /// <summary>
    /// 任务名称
    /// </summary>
    public string? JobName { get; set; }
    /// <summary>
    /// 执行状态（0-失败，1-成功）
    /// </summary>
    public int? ExecuteStatus { get; set; }
}