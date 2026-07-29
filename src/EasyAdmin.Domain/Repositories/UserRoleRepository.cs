using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasyAdmin.Domain.Repositories;

/// <summary>
/// 用户角色仓储实现
/// </summary>
public class UserRoleRepository(IConfiguration configuration, ILogger<UserRoleRepository> logger) : BaseRepositoryExt<UserRoleEntity>(configuration, logger), IUserRoleRepository
{
    /// <inheritdoc />
    public async Task<List<long>> GetUserRoleIdsAsync(long userId)
    {
        var userRoles = await QueryAsync(entity => entity.UserId == userId && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete);
        return userRoles?.Select(ur => ur.RoleId).ToList() ?? new List<long>();
    }
}