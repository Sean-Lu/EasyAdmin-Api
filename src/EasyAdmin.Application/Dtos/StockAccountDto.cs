using System.ComponentModel.DataAnnotations;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 股票账户DTO
/// </summary>
public class StockAccountDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 券商名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string BrokerName { get; set; } = string.Empty;

    /// <summary>
    /// 初始资产
    /// </summary>
    public decimal? InitialAsset { get; set; }

    /// <summary>
    /// 现资产
    /// </summary>
    public decimal? CurrentAsset { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 账户资产盈亏金额，公式：现资产 - 初始资产
    /// </summary>
    public decimal AssetProfitAmount { get; set; }

    /// <summary>
    /// 账户资产盈亏比例，公式：资产盈亏金额 / 初始资产 * 100
    /// </summary>
    public decimal AssetProfitRatio { get; set; }
}

/// <summary>
/// 股票账户更新DTO
/// </summary>
public class StockAccountUpdateDto : DtoIdBase
{
    /// <summary>
    /// 券商名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string BrokerName { get; set; } = string.Empty;

    /// <summary>
    /// 初始资产
    /// </summary>
    public decimal? InitialAsset { get; set; }

    /// <summary>
    /// 现资产
    /// </summary>
    public decimal? CurrentAsset { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Remark { get; set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; set; }
}
