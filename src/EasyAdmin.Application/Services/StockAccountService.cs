using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 股票账户服务实现
/// </summary>
public class StockAccountService(
    IMapper mapper,
    IStockAccountRepository stockAccountRepository,
    IStockHoldingRepository stockHoldingRepository
    ) : IStockAccountService
{
    public async Task<bool> AddAsync(StockAccountDto dto)
    {
        var normalized = StockAccountValidator.Normalize(dto.BrokerName, dto.InitialAsset, dto.CurrentAsset);
        var entity = mapper.Map<StockAccountEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        entity.BrokerName = normalized.BrokerName;
        entity.InitialAsset = normalized.InitialAsset;
        entity.CurrentAsset = normalized.CurrentAsset;
        return await stockAccountRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        // 有持仓的账户不能删除，避免产生孤儿持仓
        var hasHoldings = await stockHoldingRepository.ExistsAsync(entity =>
            entity.AccountId == id &&
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete);
        if (hasHoldings)
        {
            throw new ExplicitException("账户下存在持仓，不能删除");
        }

        return await stockAccountRepository.UpdateAsync(new StockAccountEntity { Id = id, IsDelete = true },
            entity => entity.IsDelete,
            entity => entity.Id == id &&
                      entity.UserId == TenantContextHolder.UserId &&
                      entity.TenantId == TenantContextHolder.TenantId &&
                      !entity.IsDelete) > 0;
    }

    public async Task<bool> UpdateAsync(StockAccountUpdateDto dto)
    {
        var normalized = StockAccountValidator.Normalize(dto.BrokerName, dto.InitialAsset, dto.CurrentAsset);
        return await stockAccountRepository.UpdateAsync(new StockAccountEntity
        {
            Id = dto.Id,
            BrokerName = normalized.BrokerName,
            InitialAsset = normalized.InitialAsset,
            CurrentAsset = normalized.CurrentAsset,
            Remark = dto.Remark
        }, entity => new { entity.BrokerName, entity.InitialAsset, entity.CurrentAsset, entity.Remark },
            entity => entity.Id == dto.Id &&
                      entity.UserId == TenantContextHolder.UserId &&
                      entity.TenantId == TenantContextHolder.TenantId &&
                      !entity.IsDelete) > 0;
    }

    public async Task<List<StockAccountDto>> ListAsync()
    {
        var orderBy = OrderByConditionBuilder<StockAccountEntity>.Build(OrderByType.Desc, entity => entity.UpdateTime,
            OrderByConditionBuilder<StockAccountEntity>.Build(OrderByType.Desc, entity => entity.CreateTime,
                OrderByConditionBuilder<StockAccountEntity>.Build(OrderByType.Desc, entity => entity.Id)));

        var entities = (await stockAccountRepository.QueryAsync(entity =>
                    entity.UserId == TenantContextHolder.UserId &&
                    entity.TenantId == TenantContextHolder.TenantId &&
                    !entity.IsDelete,
                orderBy))?.ToList() ?? new List<StockAccountEntity>();

        return entities.Select(BuildDto).ToList();
    }

    public async Task<bool> ExistsForCurrentUserAsync(long id)
    {
        return await stockAccountRepository.ExistsAsync(entity =>
            entity.Id == id &&
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete);
    }

    private StockAccountDto BuildDto(StockAccountEntity entity)
    {
        var dto = mapper.Map<StockAccountDto>(entity);
        var initialAsset = dto.InitialAsset.GetValueOrDefault();
        var currentAsset = dto.CurrentAsset.GetValueOrDefault();
        // 现资产由用户手动维护，账户盈亏按初始资产对比计算
        dto.AssetProfitAmount = Math.Round(currentAsset - initialAsset, 2);
        dto.AssetProfitRatio = initialAsset == 0
            ? 0
            : Math.Round(dto.AssetProfitAmount / initialAsset * 100, 2);
        return dto;
    }
}
