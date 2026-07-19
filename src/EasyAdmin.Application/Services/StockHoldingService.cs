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
    IStockAccountService stockAccountService
    ) : IStockHoldingService
{
    public async Task<bool> AddAsync(StockHoldingDto dto)
    {
        Validate(dto.Name, dto.Code, dto.CostPrice, dto.Quantity, dto.CurrentPrice);
        // 校验账户归属，避免跨账户写入
        await EnsureAccountAsync(dto.AccountId);

        var entity = mapper.Map<StockHoldingEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        entity.Name = dto.Name.Trim();
        entity.Code = dto.Code.Trim();
        entity.AccountId = dto.AccountId;
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
        Validate(dto.Name, dto.Code, dto.CostPrice, dto.Quantity, dto.CurrentPrice);
        await EnsureAccountAsync(dto.AccountId);

        return await stockHoldingRepository.UpdateAsync(new StockHoldingEntity
        {
            Id = dto.Id,
            AccountId = dto.AccountId,
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim(),
            CostPrice = dto.CostPrice,
            Quantity = dto.Quantity,
            CurrentPrice = dto.CurrentPrice,
            IsEnabled = dto.IsEnabled,
            SortOrder = dto.SortOrder
          }, entity => new { entity.AccountId, entity.Name, entity.Code, entity.CostPrice, entity.Quantity, entity.CurrentPrice, entity.IsEnabled, entity.SortOrder },
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
            ProfitRatio = Math.Round(profitRatio, 2),
            IsEnabled = entity.IsEnabled
        };
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
    private static void Validate(string name, string code, decimal costPrice, decimal quantity, decimal currentPrice)
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
        if (currentPrice < 0)
        {
            throw new ExplicitException("当前价格不能小于0");
        }
    }
}
