using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 股票持仓服务接口
/// </summary>
public interface IStockHoldingService
{
    /// <summary>
    /// 新增持仓
    /// </summary>
    Task<bool> AddAsync(StockHoldingDto dto);

    /// <summary>
    /// 删除持仓
    /// </summary>
    Task<bool> DeleteByIdAsync(long accountId, long id);

    /// <summary>
    /// 更新持仓
    /// </summary>
    Task<bool> UpdateAsync(StockHoldingUpdateDto dto);

    /// <summary>
    /// 更新当前价格
    /// </summary>
    Task<bool> UpdateCurrentPriceAsync(long accountId, long id, decimal currentPrice);

    /// <summary>
    /// 更新启用状态
    /// </summary>
    Task<bool> UpdateIsEnabledAsync(long accountId, long id, bool isEnabled);

    /// <summary>
    /// 获取当前用户持仓
    /// </summary>
    Task<StockHoldingListDto> ListAsync(long accountId, string? keyword);
}
