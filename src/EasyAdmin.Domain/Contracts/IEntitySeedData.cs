using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 实体种子数据接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IEntitySeedData<TEntity> where TEntity : EntityBase, new()
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    IEnumerable<TEntity> SeedData();
}