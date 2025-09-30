namespace EasyAdmin.Application.Contracts;

public interface IDtoBase
{
    /// <summary>
    /// 主键
    /// </summary>
    long Id { get; set; }

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

public interface ITreeDtoBase<TDto> : IDtoBase
{
    /// <summary>
    /// 父主键
    /// </summary>
    long PId { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    int Sort { get; set; }

    List<TDto>? Children { get; set; }
}

public interface ITenantDtoBase : IDtoBase
{
    /// <summary>
    /// 租户主键
    /// </summary>
    long TenantId { get; set; }
}

public interface ITenantTreeDtoBase<TDto> : ITenantDtoBase, ITreeDtoBase<TDto>
{
}