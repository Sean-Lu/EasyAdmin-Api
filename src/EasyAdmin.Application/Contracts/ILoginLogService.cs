using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface ILoginLogService
{
    Task<bool> AddAsync(LoginLogDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(LoginLogDto dto);
    Task<PageQueryResult<LoginLogEntity>> PageAsync(LoginLogPageReqDto request);
    Task<LoginLogEntity> GetByIdAsync(long id);
}