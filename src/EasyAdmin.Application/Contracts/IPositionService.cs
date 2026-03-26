using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IPositionService
{
    Task<bool> AddAsync(PositionDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(PositionDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<PositionEntity>> PageAsync(PositionPageReqDto request);
    Task<List<PositionEntity>> GetListAsync();
    Task<PositionEntity> GetByIdAsync(long id);
}
