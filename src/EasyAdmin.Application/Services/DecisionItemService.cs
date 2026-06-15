using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using MapsterMapper;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 随机决策候选项服务实现
/// </summary>
public class DecisionItemService(
    IMapper mapper,
    IDecisionItemRepository decisionItemRepository
    ) : IDecisionItemService
{
    public async Task<bool> AddAsync(DecisionItemDto dto)
    {
        var entity = mapper.Map<DecisionItemEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        return await decisionItemRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await decisionItemRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> UpdateAsync(DecisionItemUpdateDto dto)
    {
        return await decisionItemRepository.UpdateByDtoAsync(dto, mapper.Map<DecisionItemEntity>) > 0;
    }

    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await decisionItemRepository.UpdateAsync(new DecisionItemEntity { State = state }, item => new { item.State }, item => item.Id == id) > 0;
    }

    public async Task<List<DecisionItemDto>> GetByUserIdAsync(DecisionItemType type)
    {
        var orderBy = OrderByConditionBuilder<DecisionItemEntity>.Build(OrderByType.Asc, entity => entity.Sort,
            OrderByConditionBuilder<DecisionItemEntity>.Build(OrderByType.Asc, entity => entity.CreateTime));

        var entities = (await decisionItemRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.Type == type &&
            !entity.IsDelete,
            orderBy))?.ToList() ?? new List<DecisionItemEntity>();

        return mapper.Map<List<DecisionItemDto>>(entities);
    }

    public async Task<List<DecisionItemDto>> DrawAsync(DecisionDrawReqDto dto)
    {
        var count = Math.Max(dto.Count, 1);
        var entities = (await decisionItemRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.Type == dto.Type &&
            entity.State == CommonState.Enable &&
            !entity.IsDelete))?.ToList() ?? new List<DecisionItemEntity>();

        var results = entities
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(count, entities.Count))
            .ToList();

        return mapper.Map<List<DecisionItemDto>>(results);
    }
}
