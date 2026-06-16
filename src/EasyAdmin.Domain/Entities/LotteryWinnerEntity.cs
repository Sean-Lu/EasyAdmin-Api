using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 抽奖中奖记录表
/// </summary>
[CodeFirst]
public class LotteryWinnerEntity : TenantEntityBase
{
    /// <summary>
    /// 活动ID
    /// </summary>
    public virtual long ActivityId { get; set; }

    /// <summary>
    /// 奖项ID
    /// </summary>
    public virtual long PrizeId { get; set; }

    /// <summary>
    /// 参与人ID
    /// </summary>
    public virtual long ParticipantId { get; set; }

    /// <summary>
    /// 抽奖批次号
    /// </summary>
    [MaxLength(64)]
    public virtual string BatchNo { get; set; }

    /// <summary>
    /// 中奖人姓名快照
    /// </summary>
    [MaxLength(100)]
    public virtual string WinnerNameSnapshot { get; set; }

    /// <summary>
    /// 奖项名称快照
    /// </summary>
    [MaxLength(100)]
    public virtual string PrizeNameSnapshot { get; set; }
}
