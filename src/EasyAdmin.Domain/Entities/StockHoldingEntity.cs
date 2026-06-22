using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 股票持仓表
/// </summary>
[CodeFirst]
public class StockHoldingEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }

    /// <summary>
    /// 账户ID
    /// </summary>
    public virtual long AccountId { get; set; }

    /// <summary>
    /// 股票名称
    /// </summary>
    [MaxLength(100)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 股票代码
    /// </summary>
    [MaxLength(30)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 持仓成本
    /// </summary>
    public virtual decimal CostPrice { get; set; }

    /// <summary>
    /// 持仓数量
    /// </summary>
    public virtual decimal Quantity { get; set; }

    /// <summary>
    /// 当前价格
    /// </summary>
    public virtual decimal CurrentPrice { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;
}
