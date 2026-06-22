using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Test;

[TestClass]
public class StockHoldingCalculatorTests
{
    [TestMethod]
    public void BuildDto_Calculates_Profit_Amount_And_Ratio()
    {
        var dto = StockHoldingCalculator.BuildDto(new StockHoldingEntity
        {
            Name = "平安银行",
            Code = "000001",
            CostPrice = 10,
            Quantity = 100,
            CurrentPrice = 12.5m
        });

        Assert.AreEqual(1000, dto.CostAmount);
        Assert.AreEqual(1250, dto.MarketValue);
        Assert.AreEqual(250, dto.ProfitAmount);
        Assert.AreEqual(25, dto.ProfitRatio);
    }

    [TestMethod]
    public void BuildDto_Preserves_AccountId()
    {
        var dto = StockHoldingCalculator.BuildDto(new StockHoldingEntity
        {
            AccountId = 17,
            CostPrice = 10,
            Quantity = 100,
            CurrentPrice = 12.5m
        });

        Assert.AreEqual(17, dto.AccountId);
    }

    [TestMethod]
    public void BuildSummary_Calculates_Total_Profit_Ratio_By_Total_Cost()
    {
        var holdings = new[]
        {
            StockHoldingCalculator.BuildDto(new StockHoldingEntity { CostPrice = 10, Quantity = 100, CurrentPrice = 12.5m }),
            StockHoldingCalculator.BuildDto(new StockHoldingEntity { CostPrice = 42, Quantity = 50, CurrentPrice = 40 })
        };

        var summary = StockHoldingCalculator.BuildSummary(holdings);

        Assert.AreEqual(150, summary.TotalQuantity);
        Assert.AreEqual(3100, summary.TotalCostAmount);
        Assert.AreEqual(3250, summary.TotalMarketValue);
        Assert.AreEqual(150, summary.TotalProfitAmount);
        Assert.AreEqual(4.84m, summary.TotalProfitRatio);
    }
}
