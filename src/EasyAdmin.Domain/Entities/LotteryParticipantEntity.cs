using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 抽奖参与人表
/// </summary>
[CodeFirst]
public class LotteryParticipantEntity : TenantEntityBase
{
    /// <summary>
    /// 活动ID
    /// </summary>
    public virtual long ActivityId { get; set; }

    /// <summary>
    /// 参与人姓名
    /// </summary>
    [MaxLength(100)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 编号、手机号或工号
    /// </summary>
    [MaxLength(100)]
    public virtual string? Code { get; set; }

    /// <summary>
    /// 备注
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
