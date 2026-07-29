using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 定时任务日志仓储接口
/// </summary>
public interface IScheduleJobLogRepository : IBaseRepositoryExt<ScheduleJobLogEntity>
{

}