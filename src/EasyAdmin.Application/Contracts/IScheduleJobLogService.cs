using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IScheduleJobLogService
{
    Task<bool> AddAsync(ScheduleJobLogDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> DeleteByJobIdAsync(long jobId);
    Task<PageQueryResult<ScheduleJobLogEntity>> PageAsync(ScheduleJobLogPageReqDto request);
    Task<ScheduleJobLogEntity> GetByIdAsync(long id);
}