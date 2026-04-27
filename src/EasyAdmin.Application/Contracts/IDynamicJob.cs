namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 动态定时任务接口
/// </summary>
public interface IDynamicJob
{
    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="jobData">任务参数（JSON格式）</param>
    Task ExecuteAsync(string? jobData);
}