using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 定时任务日志服务接口
/// </summary>
public interface IScheduleJobLogService
{
    Task<bool> AddAsync(ScheduleJobLogDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> DeleteByJobIdAsync(long jobId);
    Task<PageQueryResult<ScheduleJobLogEntity>> PageAsync(ScheduleJobLogPageReqDto request);
    Task<ScheduleJobLogEntity> GetByIdAsync(long id);
}