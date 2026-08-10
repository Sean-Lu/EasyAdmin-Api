using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 股票持仓服务实现
/// </summary>
public class StockHoldingService(
    IMapper mapper,
    IStockHoldingRepository stockHoldingRepository,
    IStockAccountService stockAccountService,
    IStockQuoteProvider stockQuoteProvider
    ) : IStockHoldingService
{
    public async Task<bool> AddAsync(StockHoldingDto dto)
    {
        Validate(dto.Name, dto.Code, dto.CostPrice, dto.Quantity, dto.TargetProfitAmount, dto.CurrentPrice);
        // 校验账户归属，避免跨账户写入
        await EnsureAccountAsync(dto.AccountId);

        var entity = mapper.Map<StockHoldingEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        entity.Name = dto.Name.Trim();
        entity.Code = dto.Code.Trim();
        entity.Remark = dto.Remark?.Trim();
        entity.AccountId = dto.AccountId;
        entity.TargetProfitAmount = dto.TargetProfitAmount;
        return await stockHoldingRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long accountId, long id)
    {
        await EnsureAccountAsync(accountId);

        return await stockHoldingRepository.UpdateAsync(new StockHoldingEntity { IsDelete = true },
            entity => entity.IsDelete,
            entity => entity.Id == id &&
                      entity.AccountId == accountId &&
                      entity.UserId == TenantContextHolder.UserId &&
                      entity.TenantId == TenantContextHolder.TenantId &&
                      !entity.IsDelete) > 0;
    }

    public async Task<bool> UpdateAsync(StockHoldingUpdateDto dto)
    {
        Validate(dto.Name, dto.Code, dto.CostPrice, dto.Quantity, dto.TargetProfitAmount, dto.CurrentPrice);
        await EnsureAccountAsync(dto.AccountId);

        return await stockHoldingRepository.UpdateAsync(new StockHoldingEntity
        {
            Id = dto.Id,
            AccountId = dto.AccountId,
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim(),
            Remark = dto.Remark?.Trim(),
            CostPrice = dto.CostPrice,
            Quantity = dto.Quantity,
            TargetProfitAmount = dto.TargetProfitAmount,
            CurrentPrice = dto.CurrentPrice,
            IsEnabled = dto.IsEnabled,
            SortOrder = dto.SortOrder
          }, entity => new { entity.AccountId, entity.Name, entity.Code, entity.Remark, entity.CostPrice, entity.Quantity, entity.TargetProfitAmount, entity.CurrentPrice, entity.IsEnabled, entity.SortOrder },
              entity => entity.Id == dto.Id &&
                        entity.AccountId == dto.AccountId &&
                        entity.UserId == TenantContextHolder.UserId &&
                        entity.TenantId == TenantContextHolder.TenantId &&
                        !entity.IsDelete) > 0;
    }

    public async Task<bool> UpdateCurrentPriceAsync(long accountId, long id, decimal currentPrice)
    {
        if (currentPrice < 0)
        {
            throw new ExplicitException("当前价格不能小于0");
        }

        await EnsureAccountAsync(accountId);

        return await stockHoldingRepository.UpdateAsync(new StockHoldingEntity { Id = id, CurrentPrice = currentPrice },
            entity => entity.CurrentPrice,
            entity => entity.Id == id &&
                      entity.AccountId == accountId &&
                      entity.UserId == TenantContextHolder.UserId &&
                      entity.TenantId == TenantContextHolder.TenantId &&
                      !entity.IsDelete) > 0;
    }

    public async Task<StockHoldingPriceRefreshResultDto> RefreshCurrentPricesAsync(long accountId)
    {
        await EnsureAccountAsync(accountId);

        var entities = (await stockHoldingRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.AccountId == accountId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.IsEnabled &&
            !entity.IsDelete))?.ToList() ?? new List<StockHoldingEntity>();
        if (entities.Count == 0)
        {
            return new StockHoldingPriceRefreshResultDto();
        }

        var quotes = (await stockQuoteProvider.GetCurrentPricesAsync(entities.Select(entity => entity.Code)))
            .ToDictionary(quote => quote.StockCode, StringComparer.OrdinalIgnoreCase);
        var result = new StockHoldingPriceRefreshResultDto();

        foreach (var entity in entities)
        {
            if (!quotes.TryGetValue(entity.Code.Trim(), out var quote))
            {
                result.FailedNames.Add(entity.Name);
                continue;
            }

            var updated = await stockHoldingRepository.UpdateAsync(
                new StockHoldingEntity { Id = entity.Id, CurrentPrice = quote.CurrentPrice },
                item => item.CurrentPrice,
                item => item.Id == entity.Id &&
                        item.AccountId == accountId &&
                        item.UserId == TenantContextHolder.UserId &&
                        item.TenantId == TenantContextHolder.TenantId &&
                        !item.IsDelete) > 0;
            if (updated) result.UpdatedCount++;
            else result.FailedNames.Add(entity.Name);
        }

        if (result.UpdatedCount == 0)
        {
            throw new ExplicitException("未获取到可用的股票行情");
        }

        return result;
    }

    public async Task<bool> RefreshCurrentPriceAsync(long accountId, long id)
    {
        await EnsureAccountAsync(accountId);

        var entity = (await stockHoldingRepository.QueryAsync(item =>
            item.Id == id &&
            item.AccountId == accountId &&
            item.UserId == TenantContextHolder.UserId &&
            item.TenantId == TenantContextHolder.TenantId &&
            !item.IsDelete))?.FirstOrDefault();
        if (entity == null)
        {
            throw new ExplicitException("股票持仓不存在");
        }

        var quote = (await stockQuoteProvider.GetCurrentPricesAsync(new[] { entity.Code })).FirstOrDefault();
        if (quote == null)
        {
            throw new ExplicitException("未获取到该股票的可用行情");
        }

        return await stockHoldingRepository.UpdateAsync(
            new StockHoldingEntity { Id = id, CurrentPrice = quote.CurrentPrice },
            item => item.CurrentPrice,
            item => item.Id == id &&
                    item.AccountId == accountId &&
                    item.UserId == TenantContextHolder.UserId &&
                    item.TenantId == TenantContextHolder.TenantId &&
                    !item.IsDelete) > 0;
    }

    public async Task<bool> UpdateIsEnabledAsync(long accountId, long id, bool isEnabled)
    {
        await EnsureAccountAsync(accountId);

        return await stockHoldingRepository.UpdateAsync(new StockHoldingEntity { Id = id, IsEnabled = isEnabled },
            entity => entity.IsEnabled,
            entity => entity.Id == id &&
                      entity.AccountId == accountId &&
                      entity.UserId == TenantContextHolder.UserId &&
                      entity.TenantId == TenantContextHolder.TenantId &&
                      !entity.IsDelete) > 0;
    }

    public async Task<StockHoldingListDto> ListAsync(long accountId, string? keyword)
    {
        await EnsureAccountAsync(accountId);

        var orderBy = OrderByConditionBuilder<StockHoldingEntity>.Build(OrderByType.Asc, entity => entity.SortOrder,
            OrderByConditionBuilder<StockHoldingEntity>.Build(OrderByType.Asc, entity => entity.CreateTime));

        var normalizedKeyword = keyword?.Trim();
        var hasKeyword = !string.IsNullOrEmpty(normalizedKeyword);
        var keywordValue = normalizedKeyword ?? string.Empty;
        var entities = (await stockHoldingRepository.QueryAsync(WhereExpressionUtil.Create<StockHoldingEntity>(entity =>
                    entity.UserId == TenantContextHolder.UserId &&
                    entity.AccountId == accountId &&
                    entity.TenantId == TenantContextHolder.TenantId &&
                    !entity.IsDelete)
                .AndAlsoIF(hasKeyword, entity => entity.Name.Contains(keywordValue) || entity.Code.Contains(keywordValue)),
            orderBy))?.ToList() ?? new List<StockHoldingEntity>();

        var list = entities.Select(BuildDto).ToList();
        return new StockHoldingListDto
        {
            List = list,
            Summary = BuildSummary(list)
        };
    }

    private static StockHoldingDto BuildDto(StockHoldingEntity entity)
    {
        var costAmount = entity.CostPrice * entity.Quantity;
        var marketValue = entity.CurrentPrice * entity.Quantity;
        var profitAmount = (entity.CurrentPrice - entity.CostPrice) * entity.Quantity;
        var profitRatio = entity.CostPrice == 0 ? 0 : (entity.CurrentPrice - entity.CostPrice) / entity.CostPrice * 100;
        var targetPrice = CalculateTargetPrice(entity.CostPrice, entity.Quantity, entity.TargetProfitAmount);

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
            Remark = entity.Remark,
            CostPrice = entity.CostPrice,
            Quantity = entity.Quantity,
            TargetProfitAmount = entity.TargetProfitAmount,
            CurrentPrice = entity.CurrentPrice,
            CostAmount = Math.Round(costAmount, 2),
            MarketValue = Math.Round(marketValue, 2),
            ProfitAmount = Math.Round(profitAmount, 2),
            ProfitRatio = Math.Round(profitRatio, 2),
            TargetPrice = targetPrice,
            IsEnabled = entity.IsEnabled
        };
    }

    private static decimal? CalculateTargetPrice(decimal costPrice, decimal quantity, decimal? targetProfitAmount)
    {
        if (!targetProfitAmount.HasValue || quantity <= 0)
        {
            return null;
        }

        var targetPrice = costPrice + targetProfitAmount.Value / quantity;
        return Math.Ceiling(targetPrice * 1000) / 1000;
    }

    private static StockHoldingSummaryDto BuildSummary(IEnumerable<StockHoldingDto> holdings)
    {
        var items = holdings.Where(item => item.IsEnabled).ToList();
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

    private async Task EnsureAccountAsync(long accountId)
    {
        if (accountId < 1 || !await stockAccountService.ExistsForCurrentUserAsync(accountId))
        {
            throw new ExplicitException("股票账户不存在");
        }
    }

    /// <summary>
    /// 校验持仓基础字段
    /// </summary>
    private static void Validate(string name, string code, decimal costPrice, decimal quantity, decimal? targetProfitAmount, decimal currentPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ExplicitException("股票名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ExplicitException("股票代码不能为空");
        }
        if (costPrice < 0)
        {
            throw new ExplicitException("持仓成本不能小于0");
        }
        if (quantity < 0)
        {
            throw new ExplicitException("持仓数量不能小于0");
        }
        if (targetProfitAmount.HasValue && targetProfitAmount <= 0)
        {
            throw new ExplicitException("目标盈利金额必须大于0");
        }
        if (targetProfitAmount.HasValue && quantity == 0)
        {
            throw new ExplicitException("设置目标盈利金额时持仓数量必须大于0");
        }
        if (currentPrice < 0)
        {
            throw new ExplicitException("当前价格不能小于0");
        }
    }

}
