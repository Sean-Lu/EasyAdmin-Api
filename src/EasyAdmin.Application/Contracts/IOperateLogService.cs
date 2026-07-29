using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 操作日志服务接口
/// </summary>
public interface IOperateLogService
{
    Task<bool> AddAsync(OperateLogDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<PageQueryResult<OperateLogEntity>> PageAsync(OperateLogPageReqDto request);
    Task<OperateLogEntity> GetByIdAsync(long id);
}