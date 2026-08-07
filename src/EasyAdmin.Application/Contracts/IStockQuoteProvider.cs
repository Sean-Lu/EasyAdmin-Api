namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 股票行情提供者
/// </summary>
public interface IStockQuoteProvider
{
    /// <summary>
    /// 获取股票当前价格
    /// </summary>
    Task<IReadOnlyList<StockQuote>> GetCurrentPricesAsync(IEnumerable<string> stockCodes);
}

/// <summary>
/// 股票行情
/// </summary>
public record StockQuote(string StockCode, decimal CurrentPrice);
