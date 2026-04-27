namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 定时任务分页查询请求 DTO
/// </summary>
public class ScheduleJobPageReqDto : PageRequestBase
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string? JobName { get; set; }
    /// <summary>
    /// 任务组
    /// </summary>
    public string? JobGroup { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public int? State { get; set; }
}