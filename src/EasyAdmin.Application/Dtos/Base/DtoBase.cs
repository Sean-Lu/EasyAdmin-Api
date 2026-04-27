using EasyAdmin.Application.Contracts;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 基础DTO类（只包含ID字段）
/// </summary>
public abstract class IdBase : IIdBase
{
    /// <summary>
    /// 主键
    /// </summary>
    public virtual long Id { get; set; }
}

/// <summary>
/// 基础DTO类
/// </summary>
public abstract class DtoBase : IdBase, IDtoBase
{
    /// <summary>
    /// 创建用户主键
    /// </summary>
    public virtual long CreateUserId { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }
    /// <summary>
    /// 更新用户主键
    /// </summary>
    public virtual long UpdateUserId { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public virtual DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 逻辑删除标记
    /// </summary>
    public virtual bool IsDelete { get; set; }
}

public abstract class TreeDtoBase<TDto> : DtoBase, ITreeDtoBase<TDto>
{
    /// <summary>
    /// 父主键
    /// </summary>
    public virtual long PId { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public virtual List<TDto>? Children { get; set; }
}

public abstract class TenantDtoBase : DtoBase, ITenantDtoBase
{
    /// <summary>
    /// 租户主键
    /// </summary>
    public virtual long TenantId { get; set; }
}

public abstract class TenantTreeDtoBase<TDto> : TenantDtoBase, ITenantTreeDtoBase<TDto>
{
    /// <summary>
    /// 父主键
    /// </summary>
    public virtual long PId { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public List<TDto>? Children { get; set; }
}
