using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 抽奖活动表
/// </summary>
[CodeFirst]
public class LotteryActivityEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }

    /// <summary>
    /// 活动名称
    /// </summary>
    [MaxLength(100)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 活动说明
    /// </summary>
    [MaxLength(500)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// 是否允许同一参与人重复中奖
    /// </summary>
    [DefaultValue(false)]
    public virtual bool AllowRepeatWinner { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}
