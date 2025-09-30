using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Domain.Contracts;

public interface IEntitySeedData<TEntity> where TEntity : EntityBase, new()
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    IEnumerable<TEntity> SeedData();
}