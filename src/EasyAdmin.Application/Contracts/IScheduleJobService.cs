using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IScheduleJobService
{
    Task<bool> AddAsync(ScheduleJobDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(ScheduleJobUpdateDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<ScheduleJobEntity>> PageAsync(ScheduleJobPageReqDto request);
    Task<ScheduleJobEntity> GetByIdAsync(long id);
    Task<bool> RunOnceAsync(long id);
    Task<List<ScheduleJobEntity>> GetEnabledJobsAsync();
    Task<bool> UpdateExecuteTimeAsync(long id, DateTime? lastExecuteTime, DateTime? nextExecuteTime);
    
    // Quartz 同步方法
    Task<bool> SyncToQuartzAsync(long jobId);
    Task<bool> SyncToQuartzAsync(List<long> jobIds);
}
