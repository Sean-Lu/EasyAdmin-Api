using System.Data;
using System.Linq.Expressions;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 基础仓储扩展接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IBaseRepositoryExt<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// 按主键删除
    /// </summary>
    bool DeleteById(long id, IDbTransaction? transaction = null);

    /// <summary>
    /// 按主键批量删除
    /// </summary>
    bool DeleteByIds(IEnumerable<long> ids, IDbTransaction? transaction = null);

    /// <summary>
    /// 按主键查询
    /// </summary>
    TEntity GetById(long id);

    /// <summary>
    /// 按主键批量查询
    /// </summary>
    List<TEntity>? GetByIds(IEnumerable<long> ids);

    /// <summary>
    /// 获取最后更新时间
    /// </summary>
    DateTime? GetLastUpdateTime(Expression<Func<TEntity, bool>>? whereExpression = null);

    /// <summary>
    /// 按主键异步删除
    /// </summary>
    Task<bool> DeleteByIdAsync(long id, IDbTransaction? transaction = null);

    /// <summary>
    /// 按主键批量异步删除
    /// </summary>
    Task<bool> DeleteByIdsAsync(IEnumerable<long> ids, IDbTransaction? transaction = null);

    /// <summary>
    /// 按主键异步查询
    /// </summary>
    Task<TEntity> GetByIdAsync(long id);

    /// <summary>
    /// 按主键批量异步查询
    /// </summary>
    Task<List<TEntity>?> GetByIdsAsync(IEnumerable<long> ids);

    /// <summary>
    /// 异步获取最后更新时间
    /// </summary>
    Task<DateTime?> GetLastUpdateTimeAsync(Expression<Func<TEntity, bool>>? whereExpression = null);
}