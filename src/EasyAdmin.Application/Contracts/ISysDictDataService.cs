using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 字典数据服务接口
/// </summary>
public interface ISysDictDataService
{
    Task<bool> AddAsync(SysDictDataDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(SysDictDataUpdateDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<SysDictDataEntity>> PageAsync(SysDictDataPageReqDto request);
    Task<List<SysDictDataEntity>> GetByTypeCodeAsync(string typeCode);
    Task<SysDictDataEntity> GetByIdAsync(long id);
}