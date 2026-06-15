using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 随机决策候选项表
/// </summary>
[CodeFirst]
public class DecisionItemEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }

    /// <summary>
    /// 决策类型
    /// </summary>
    public virtual DecisionItemType Type { get; set; }

    /// <summary>
    /// 候选项名称
    /// </summary>
    [MaxLength(100)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [DefaultValue(0)]
    public virtual int Sort { get; set; }
}
