using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface ISysDictTypeService
{
    Task<bool> AddAsync(SysDictTypeDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(SysDictTypeDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<SysDictTypeEntity>> PageAsync(SysDictTypePageReqDto request);
    Task<List<SysDictTypeEntity>> GetAllAsync();
    Task<SysDictTypeEntity> GetByIdAsync(long id);
    Task<SysDictTypeEntity> GetByCodeAsync(string code);
}