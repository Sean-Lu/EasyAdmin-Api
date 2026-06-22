using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Services;

public static class StockHoldingCalculator
{
    public static StockHoldingDto BuildDto(StockHoldingEntity entity)
    {
        var costAmount = entity.CostPrice * entity.Quantity;
        var marketValue = entity.CurrentPrice * entity.Quantity;
        var profitAmount = (entity.CurrentPrice - entity.CostPrice) * entity.Quantity;
        var profitRatio = entity.CostPrice == 0 ? 0 : (entity.CurrentPrice - entity.CostPrice) / entity.CostPrice * 100;

        return new StockHoldingDto
        {
            Id = entity.Id,
            CreateUserId = entity.CreateUserId,
            CreateTime = entity.CreateTime,
            UpdateUserId = entity.UpdateUserId,
            UpdateTime = entity.UpdateTime,
            IsDelete = entity.IsDelete,
            TenantId = entity.TenantId,
            UserId = entity.UserId,
            AccountId = entity.AccountId,
            Name = entity.Name,
            Code = entity.Code,
            CostPrice = entity.CostPrice,
            Quantity = entity.Quantity,
            CurrentPrice = entity.CurrentPrice,
            CostAmount = Math.Round(costAmount, 2),
            MarketValue = Math.Round(marketValue, 2),
            ProfitAmount = Math.Round(profitAmount, 2),
            ProfitRatio = Math.Round(profitRatio, 2)
        };
    }

    public static StockHoldingSummaryDto BuildSummary(IEnumerable<StockHoldingDto> holdings)
    {
        var items = holdings.ToList();
        var totalCostAmount = items.Sum(item => item.CostAmount);
        var totalProfitAmount = items.Sum(item => item.ProfitAmount);
        var totalProfitRatio = totalCostAmount == 0 ? 0 : totalProfitAmount / totalCostAmount * 100;

        return new StockHoldingSummaryDto
        {
            TotalQuantity = items.Sum(item => item.Quantity),
            TotalCostAmount = Math.Round(totalCostAmount, 2),
            TotalMarketValue = Math.Round(items.Sum(item => item.MarketValue), 2),
            TotalProfitAmount = Math.Round(totalProfitAmount, 2),
            TotalProfitRatio = Math.Round(totalProfitRatio, 2)
        };
    }
}
