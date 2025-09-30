using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface ISysDictService
{
    Task<bool> AddAsync(SysDictDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(SysDictDto dto);
    Task<PageQueryResult<SysDictEntity>> PageAsync(SysDictPageReqDto request);
    Task<SysDictEntity> GetByIdAsync(long id);
}