using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface ITenantService
{
    Task<bool> AddAsync(TenantDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(TenantDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<TenantEntity>> PageAsync(TenantPageReqDto request);
    Task<TenantEntity> GetByIdAsync(long id);
    Task<TenantEntity> GetByNameAsync(string name);
}