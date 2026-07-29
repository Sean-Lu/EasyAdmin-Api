using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 用户角色仓储接口
/// </summary>
public interface IUserRoleRepository : IBaseRepositoryExt<UserRoleEntity>
{
    /// <summary>
    /// 获取用户角色ID
    /// </summary>
    Task<List<long>> GetUserRoleIdsAsync(long userId);
}