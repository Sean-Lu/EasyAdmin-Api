namespace EasyAdmin.Domain.Contracts;

/// <summary>
/// 基础ID接口（只包含ID字段）
/// </summary>
public interface IIdBase
{
    /// <summary>
    /// 主键
    /// </summary>
    long Id { get; set; }
}

/// <summary>
/// 树ID接口
/// </summary>
public interface ITreeIdBase : IIdBase
{
    /// <summary>
    /// 父主键
    /// </summary>
    long PId { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    int Sort { get; set; }
}

/// <summary>
/// 基础实体接口
/// </summary>
public interface IEntityBase : IIdBase
{
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

/// <summary>
/// 树实体接口
/// </summary>
public interface ITreeEntityBase<TEntity> : ITreeIdBase, IEntityBase
{
    /// <summary>
    /// 子节点数据
    /// </summary>
    List<TEntity>? Children { get; set; }
}

/// <summary>
/// 租户实体接口
/// </summary>
public interface ITenantEntityBase : IEntityBase
{
    /// <summary>
    /// 租户ID
    /// </summary>
    long TenantId { get; set; }
}

/// <summary>
/// 租户+树实体接口
/// </summary>
public interface ITenantTreeEntityBase<TEntity> : ITenantEntityBase, ITreeEntityBase<TEntity>
{
}