namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 任务列表分页查询条件
/// </summary>
public class TaskPageReqDto : PageRequestBase
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string? TaskName { get; set; }
    /// <summary>
    /// 任务类型
    /// </summary>
    public int? TaskType { get; set; }
}