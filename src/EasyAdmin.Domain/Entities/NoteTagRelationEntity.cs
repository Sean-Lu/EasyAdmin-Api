using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 笔记标签关系表
/// </summary>
[CodeFirst]
public class NoteTagRelationEntity : TenantEntityBase
{
    /// <summary>
    /// 笔记ID
    /// </summary>
    public virtual long NoteId { get; set; }
    /// <summary>
    /// 标签ID
    /// </summary>
    public virtual long TagId { get; set; }
}
