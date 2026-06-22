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
            CurrentPrice = dto.CurrentPrice
          }, entity => new { entity.AccountId, entity.Name, entity.Code, entity.CostPrice, entity.Quantity, entity.CurrentPrice },
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

    public async Task<StockHoldingListDto> ListAsync(long accountId, string? keyword)
    {
        await EnsureAccountAsync(accountId);

        var orderBy = OrderByConditionBuilder<StockHoldingEntity>.Build(OrderByType.Desc, entity => entity.UpdateTime,
            OrderByConditionBuilder<StockHoldingEntity>.Build(OrderByType.Desc, entity => entity.CreateTime,
                OrderByConditionBuilder<StockHoldingEntity>.Build(OrderByType.Desc, entity => entity.Id)));

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

        var list = entities.Select(StockHoldingCalculator.BuildDto).ToList();
        return new StockHoldingListDto
        {
            List = list,
            Summary = StockHoldingCalculator.BuildSummary(list)
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
