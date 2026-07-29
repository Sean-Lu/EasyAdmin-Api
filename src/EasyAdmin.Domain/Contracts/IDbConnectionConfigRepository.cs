using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 数据库连接配置仓储接口
/// </summary>
public interface IDbConnectionConfigRepository : IBaseRepositoryExt<DbConnectionConfigEntity>
{
}
