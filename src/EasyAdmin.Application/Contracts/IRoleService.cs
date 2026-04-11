using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IRoleService
{
    Task<bool> AddAsync(RoleDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(RoleDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    Task<PageQueryResult<RoleEntity>> PageAsync(RolePageReqDto request);
    Task<List<RoleEntity>> GetAllRolesAsync();
    Task<RoleEntity> GetByIdAsync(long id);
    
    // 角色菜单权限相关
    Task<bool> AssignMenusToRoleAsync(RoleMenuAssignmentDto dto);
    Task<List<long>> GetRoleMenuIdsAsync(long roleId);
    Task<List<RoleMenuEntity>> GetRoleMenusAsync(long roleId);
}