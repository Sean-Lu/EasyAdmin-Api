using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 租户仓储接口
/// </summary>
public interface ITenantRepository : IBaseRepositoryExt<TenantEntity>
{

}