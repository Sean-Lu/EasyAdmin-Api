using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

public interface IMenuService
{
    Task<bool> AddAsync(MenuDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(MenuDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<List<MenuEntity>?> GetMenuTreeAsync(MenuListReqDto request);
    Task<MenuEntity> GetByIdAsync(long id);
}