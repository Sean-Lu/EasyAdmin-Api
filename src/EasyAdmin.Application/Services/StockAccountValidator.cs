using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 股票账户入参校验
/// </summary>
public static class StockAccountValidator
{
    /// <summary>
    /// 校验券商名称和账户资产
    /// </summary>
    public static StockAccountNormalizedValue Normalize(string? brokerName, decimal? initialAsset, decimal? currentAsset)
    {
        if (string.IsNullOrWhiteSpace(brokerName))
        {
            throw new ExplicitException("券商名称不能为空");
        }

        var normalizedInitialAsset = initialAsset ?? 0;
        if (normalizedInitialAsset < 0)
        {
            throw new ExplicitException("初始资产不能小于0");
        }

        var normalizedCurrentAsset = currentAsset ?? 0;
        if (normalizedCurrentAsset < 0)
        {
            throw new ExplicitException("现资产不能小于0");
        }

        return new StockAccountNormalizedValue(brokerName.Trim(), normalizedInitialAsset, normalizedCurrentAsset);
    }
}

/// <summary>
/// 股票账户校验后的规范化值
/// </summary>
public sealed record StockAccountNormalizedValue(string BrokerName, decimal InitialAsset, decimal CurrentAsset);
