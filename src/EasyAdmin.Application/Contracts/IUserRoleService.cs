using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 用户角色服务接口
/// </summary>
public interface IUserRoleService
{
    // 用户角色分配相关
    Task<bool> AssignRolesToUserAsync(UserRoleAssignmentDto dto);
    Task<List<long>> GetUserRoleIdsAsync(long userId);
    Task<List<RoleEntity>> GetUserRolesAsync(long userId);
}