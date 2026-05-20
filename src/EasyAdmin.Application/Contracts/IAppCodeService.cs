using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IAppCodeService
{
    Task<bool> AddAsync(AppCodeAddDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> UpdateAsync(AppCodeUpdateDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<AppCodeEntity>> PageAsync(AppCodePageReqDto request);
    Task<AppCodeEntity> GetByIdAsync(long id);
    Task<List<AppCodeDto>> GetActiveListAsync();
}