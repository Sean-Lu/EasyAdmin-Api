using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IOperateLogService
{
    Task<bool> AddAsync(OperateLogDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(OperateLogDto dto);
    Task<PageQueryResult<OperateLogEntity>> PageAsync(OperateLogPageReqDto request);
    Task<OperateLogEntity> GetByIdAsync(long id);
}