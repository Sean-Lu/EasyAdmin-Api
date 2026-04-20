using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Application.Services;

public class UserRoleService(
    ILogger<UserRoleService> logger,
    IUserRoleRepository userRoleRepository,
    IRoleRepository roleRepository
    ) : IUserRoleService
{
    public async Task<bool> AssignRolesToUserAsync(UserRoleAssignmentDto dto)
    {
        // 使用自动事务（自动提交或回滚）
        return await userRoleRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            // 先删除该用户的所有角色关联
            await userRoleRepository.DeleteAsync(entity => entity.UserId == dto.UserId && entity.TenantId == TenantContextHolder.TenantId, transaction);

            // 添加新的角色关联
            if (dto.RoleIds.Any())
            {
                var userRoles = dto.RoleIds.Select(roleId => new UserRoleEntity
                {
                    UserId = dto.UserId,
                    RoleId = roleId
                }).ToList();
                await userRoleRepository.AddAsync(userRoles, transaction: transaction);
            }

            return true;
        });
    }

    public async Task<List<long>> GetUserRoleIdsAsync(long userId)
    {
        return await userRoleRepository.GetUserRoleIdsAsync(userId);
    }

    public async Task<List<RoleEntity>> GetUserRolesAsync(long userId)
    {
        var userRoleIds = await GetUserRoleIdsAsync(userId);
        if (!userRoleIds.Any())
        {
            return new List<RoleEntity>();
        }

        var roles = await roleRepository.QueryAsync(entity => userRoleIds.Contains(entity.Id) && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete);
        return roles?.ToList() ?? new List<RoleEntity>();
    }
}