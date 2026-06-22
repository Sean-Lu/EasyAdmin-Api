using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 股票持仓DTO
/// </summary>
public class StockHoldingDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 账户ID
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// 股票名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    /// <summary>
    /// 股票代码
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Code { get; set; }

    /// <summary>
    /// 持仓成本
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 持仓数量
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 当前价格
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// 成本金额
    /// </summary>
    public decimal CostAmount { get; set; }

    /// <summary>
    /// 持仓市值
    /// </summary>
    public decimal MarketValue { get; set; }

    /// <summary>
    /// 盈亏金额
    /// </summary>
    public decimal ProfitAmount { get; set; }

    /// <summary>
    /// 盈亏比例
    /// </summary>
    public decimal ProfitRatio { get; set; }
}

/// <summary>
/// 股票持仓更新DTO
/// </summary>
public class StockHoldingUpdateDto : DtoIdBase
{
    /// <summary>
    /// 账户ID
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// 股票名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    /// <summary>
    /// 股票代码
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Code { get; set; }

    /// <summary>
    /// 持仓成本
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 持仓数量
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 当前价格
    /// </summary>
    public decimal CurrentPrice { get; set; }
}

/// <summary>
/// 股票持仓统计DTO
/// </summary>
public class StockHoldingSummaryDto
{
    /// <summary>
    /// 总持仓数量
    /// </summary>
    public decimal TotalQuantity { get; set; }

    /// <summary>
    /// 总成本金额
    /// </summary>
    public decimal TotalCostAmount { get; set; }

    /// <summary>
    /// 总持仓市值
    /// </summary>
    public decimal TotalMarketValue { get; set; }

    /// <summary>
    /// 总盈亏金额
    /// </summary>
    public decimal TotalProfitAmount { get; set; }

    /// <summary>
    /// 总盈亏比例
    /// </summary>
    public decimal TotalProfitRatio { get; set; }
}

/// <summary>
/// 股票持仓列表DTO
/// </summary>
public class StockHoldingListDto
{
    /// <summary>
    /// 持仓列表
    /// </summary>
    public List<StockHoldingDto> List { get; set; } = new();

    /// <summary>
    /// 持仓统计
    /// </summary>
    public StockHoldingSummaryDto Summary { get; set; } = new();
}
