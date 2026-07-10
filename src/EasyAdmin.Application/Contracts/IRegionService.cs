using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

public interface IRegionService
{
    Task<bool> AddAsync(RegionDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(RegionUpdateDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<List<RegionEntity>> GetRegionTreeAsync(RegionListReqDto request);
    Task<RegionEntity> GetByIdAsync(long id);
}
