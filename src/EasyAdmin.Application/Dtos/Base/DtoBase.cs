using EasyAdmin.Application.Contracts;

namespace EasyAdmin.Application.Dtos;

public abstract class DtoBase : IDtoBase
{
    public virtual long Id { get; set; }

    public virtual long CreateUserId { get; set; }
    public virtual DateTime? CreateTime { get; set; }
    public virtual long UpdateUserId { get; set; }
    public virtual DateTime? UpdateTime { get; set; }

    public virtual bool IsDelete { get; set; }
}

public abstract class TreeDtoBase<TDto> : DtoBase, ITreeDtoBase<TDto>
{
    public virtual long PId { get; set; }
    public int Sort { get; set; }

    public virtual List<TDto>? Children { get; set; }
}

public abstract class TenantDtoBase : DtoBase, ITenantDtoBase
{
    public virtual long TenantId { get; set; }
}

public abstract class TenantTreeDtoBase<TDto> : TenantDtoBase, ITenantTreeDtoBase<TDto>
{
    public virtual long PId { get; set; }
    public int Sort { get; set; }

    public List<TDto>? Children { get; set; }
}