using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 定时任务仓储接口
/// </summary>
public interface IScheduleJobRepository : IBaseRepositoryExt<ScheduleJobEntity>
{

}