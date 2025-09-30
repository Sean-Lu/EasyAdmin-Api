namespace EasyAdmin.Domain.Contracts;

public interface IEntityBase
{
    /// <summary>
    /// 主键
    /// </summary>
    long Id { get; set; }

    /// <summary>
    /// 创建人ID
    /// </summary>
    long CreateUserId { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime? CreateTime { get; set; }
    /// <summary>
    /// 更新人ID
    /// </summary>
    long UpdateUserId { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    bool IsDelete { get; set; }
}

public interface ITreeEntityBase<TEntity> : IEntityBase
{
    /// <summary>
    /// 父主键
    /// </summary>
    long PId { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    int Sort { get; set; }

    /// <summary>
    /// 子节点数据
    /// </summary>
    List<TEntity>? Children { get; set; }
}

public interface ITenantEntityBase : IEntityBase
{
    /// <summary>
    /// 租户ID
    /// </summary>
    long TenantId { get; set; }
}

public interface ITenantTreeEntityBase<TEntity> : ITenantEntityBase, ITreeEntityBase<TEntity>
{
}