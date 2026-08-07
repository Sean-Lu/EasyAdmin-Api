using System.Text.RegularExpressions;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 腾讯股票行情提供者
/// </summary>
public class TencentStockQuoteProvider(IHttpClientFactory httpClientFactory) : IStockQuoteProvider
{
    public async Task<IReadOnlyList<StockQuote>> GetCurrentPricesAsync(IEnumerable<string> stockCodes)
    {
        var codeSymbols = stockCodes
            .Select(code => (StockCode: code.Trim(), Symbol: NormalizeSymbol(code)))
            .Where(item => !string.IsNullOrEmpty(item.StockCode))
            .GroupBy(item => item.Symbol, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
        if (codeSymbols.Count == 0)
        {
            return Array.Empty<StockQuote>();
        }

        string responseText;
        try
        {
            responseText = await httpClientFactory.CreateClient("StockQuote").GetStringAsync(
                $"https://qt.gtimg.cn/q={string.Join(',', codeSymbols.Select(item => item.Symbol))}");
        }
        catch (HttpRequestException)
        {
            throw new ExplicitException("股票行情获取失败，请稍后重试");
        }
        catch (TaskCanceledException)
        {
            throw new ExplicitException("股票行情获取超时，请稍后重试");
        }

        var quotes = new List<StockQuote>();
        foreach (var item in codeSymbols)
        {
            var match = Regex.Match(responseText, $"v_{Regex.Escape(item.Symbol)}=\\\"([^\\\"]*)\\\"");
            var fields = match.Success ? match.Groups[1].Value.Split('~') : Array.Empty<string>();
            if (fields.Length > 3 && decimal.TryParse(fields[3], out var currentPrice) && currentPrice >= 0)
            {
                quotes.Add(new StockQuote(item.StockCode, currentPrice));
            }
        }

        return quotes;
    }

    private static string NormalizeSymbol(string code)
    {
        var normalized = code.Trim().ToLowerInvariant();
        if (normalized.StartsWith("sh") || normalized.StartsWith("sz") || normalized.StartsWith("bj"))
        {
            return normalized;
        }

        var digits = new string(normalized.Where(char.IsDigit).ToArray());
        if (digits.Length != 6)
        {
            return normalized;
        }

        var prefix = digits[0] == '6' ? "sh" : digits[0] is '4' or '8' ? "bj" : "sz";
        return prefix + digits;
    }
}
