namespace EasyAdmin.Application.Dtos;

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