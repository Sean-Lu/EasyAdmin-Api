using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 抽奖奖项表
/// </summary>
[CodeFirst]
public class LotteryPrizeEntity : TenantEntityBase
{
    /// <summary>
    /// 活动ID
    /// </summary>
    public virtual long ActivityId { get; set; }

    /// <summary>
    /// 奖项名称
    /// </summary>
    [MaxLength(100)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 中奖名额
    /// </summary>
    public virtual int Quota { get; set; }

    /// <summary>
    /// 奖项说明
    /// </summary>
    [MaxLength(500)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [DefaultValue(0)]
    public virtual int Sort { get; set; }
}
