using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 待办事项表
/// </summary>
[CodeFirst]
public class TodoItemEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }
    /// <summary>
    /// 分类ID
    /// </summary>
    public virtual long CategoryId { get; set; }
    /// <summary>
    /// 待办事项名称
    /// </summary>
    [MaxLength(100)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 是否完成
    /// </summary>
    [DefaultValue(false)]
    public virtual bool Done { get; set; }
    /// <summary>
    /// 优先级（1-低，2-中，3-高）
    /// </summary>
    [DefaultValue(1)]
    public virtual int Priority { get; set; }
    /// <summary>
    /// 排序顺序
    /// </summary>
    [DefaultValue(0)]
    public virtual int SortOrder { get; set; }
}