using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 待办事项分类表
/// </summary>
[CodeFirst]
public class TodoCategoryEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 分类名称
    /// </summary>
    [MaxLength(50)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 排序顺序
    /// </summary>
    [DefaultValue(0)]
    public virtual int SortOrder { get; set; }
}
