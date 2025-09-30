using System.Data;
using System.Linq.Expressions;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Contracts;

public interface IBaseRepositoryExt<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    bool DeleteById(long id, IDbTransaction? transaction = null);
    bool DeleteByIds(IEnumerable<long> ids, IDbTransaction? transaction = null);

    TEntity GetById(long id);
    List<TEntity>? GetByIds(IEnumerable<long> ids);

    DateTime? GetLastUpdateTime(Expression<Func<TEntity, bool>>? whereExpression = null);

    Task<bool> DeleteByIdAsync(long id, IDbTransaction? transaction = null);
    Task<bool> DeleteByIdsAsync(IEnumerable<long> ids, IDbTransaction? transaction = null);

    Task<TEntity> GetByIdAsync(long id);
    Task<List<TEntity>?> GetByIdsAsync(IEnumerable<long> ids);

    Task<DateTime?> GetLastUpdateTimeAsync(Expression<Func<TEntity, bool>>? whereExpression = null);
}