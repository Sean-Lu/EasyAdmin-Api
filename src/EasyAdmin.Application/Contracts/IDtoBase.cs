using EasyAdmin.Domain.Contracts;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 基础DTO接口
/// </summary>
public interface IDtoBase : IIdBase
{
    /// <summary>
    /// 创建用户主键
    /// </summary>
    long CreateUserId { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime? CreateTime { get; set; }
    /// <summary>
    /// 更新用户主键
    /// </summary>
    long UpdateUserId { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 逻辑删除标记
    /// </summary>
    bool IsDelete { get; set; }
}

/// <summary>
/// 树DTO接口
/// </summary>
public interface ITreeDtoBase<TDto> : ITreeIdBase, IDtoBase
{
    List<TDto>? Children { get; set; }
}

/// <summary>
/// 租户DTO接口
/// </summary>
public interface ITenantDtoBase : IDtoBase
{
    /// <summary>
    /// 租户主键
    /// </summary>
    long TenantId { get; set; }
}

/// <summary>
/// 租户+树DTO接口
/// </summary>
public interface ITenantTreeDtoBase<TDto> : ITenantDtoBase, ITreeDtoBase<TDto>
{
}
