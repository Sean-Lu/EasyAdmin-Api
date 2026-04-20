using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

public interface IUserRoleRepository : IBaseRepositoryExt<UserRoleEntity>
{
    Task<List<long>> GetUserRoleIdsAsync(long userId);
}