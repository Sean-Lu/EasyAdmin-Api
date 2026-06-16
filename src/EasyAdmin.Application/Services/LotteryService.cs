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
/// 抽奖服务实现
/// </summary>
public class LotteryService(
    IMapper mapper,
    ILotteryActivityRepository lotteryActivityRepository,
    ILotteryPrizeRepository lotteryPrizeRepository,
    ILotteryParticipantRepository lotteryParticipantRepository,
    ILotteryWinnerRepository lotteryWinnerRepository
    ) : ILotteryService
{
    public async Task<List<LotteryActivityDto>> GetActivityListAsync()
    {
        var orderBy = OrderByConditionBuilder<LotteryActivityEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        var entities = (await lotteryActivityRepository.QueryAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete,
            orderBy))?.ToList() ?? new List<LotteryActivityEntity>();

        return mapper.Map<List<LotteryActivityDto>>(entities);
    }

    public async Task<LotteryActivityDetailDto> GetActivityDetailAsync(long activityId)
    {
        var activity = await GetOwnedActivityAsync(activityId);
        if (activity == null)
        {
            return new LotteryActivityDetailDto();
        }

        return new LotteryActivityDetailDto
        {
            Activity = mapper.Map<LotteryActivityDto>(activity),
            Prizes = await GetPrizeListAsync(activityId),
            Participants = await GetParticipantListAsync(activityId),
            Winners = await GetWinnerListAsync(activityId)
        };
    }

    public async Task<bool> AddActivityAsync(LotteryActivityDto dto)
    {
        var entity = mapper.Map<LotteryActivityEntity>(dto);
        entity.UserId = TenantContextHolder.UserId;
        entity.State = dto.State;
        return await lotteryActivityRepository.AddAsync(entity);
    }

    public async Task<bool> UpdateActivityAsync(LotteryActivityUpdateDto dto)
    {
        var existing = await GetOwnedActivityAsync(dto.Id);
        if (existing == null)
        {
            return false;
        }

        return await lotteryActivityRepository.UpdateAsync(new LotteryActivityEntity
        {
            Id = dto.Id,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            AllowRepeatWinner = dto.AllowRepeatWinner,
            State = dto.State
        }, entity => new { entity.Name, entity.Description, entity.AllowRepeatWinner, entity.State }) > 0;
    }

    public async Task<bool> DeleteActivityAsync(long id)
    {
        var existing = await GetOwnedActivityAsync(id);
        if (existing == null)
        {
            return false;
        }

        return await lotteryActivityRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            // 删除活动时保留审计痕迹，依赖各 Repository 的逻辑删除能力级联标记子数据。
            await lotteryWinnerRepository.DeleteAsync(entity => entity.ActivityId == id && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await lotteryParticipantRepository.DeleteAsync(entity => entity.ActivityId == id && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await lotteryPrizeRepository.DeleteAsync(entity => entity.ActivityId == id && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await lotteryActivityRepository.DeleteByIdAsync(id, transaction);
            return true;
        });
    }

    public async Task<List<LotteryPrizeDto>> GetPrizeListAsync(long activityId)
    {
        if (await GetOwnedActivityAsync(activityId) == null)
        {
            return new List<LotteryPrizeDto>();
        }

        var orderBy = OrderByConditionBuilder<LotteryPrizeEntity>.Build(OrderByType.Asc, entity => entity.Sort,
            OrderByConditionBuilder<LotteryPrizeEntity>.Build(OrderByType.Asc, entity => entity.CreateTime));
        var entities = (await lotteryPrizeRepository.QueryAsync(entity =>
            entity.ActivityId == activityId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete,
            orderBy))?.ToList() ?? new List<LotteryPrizeEntity>();
        var dtos = mapper.Map<List<LotteryPrizeDto>>(entities);
        var winners = (await lotteryWinnerRepository.QueryAsync(entity =>
            entity.ActivityId == activityId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<LotteryWinnerEntity>();
        var winnerCountMap = winners.GroupBy(entity => entity.PrizeId).ToDictionary(group => group.Key, group => group.Count());

        foreach (var dto in dtos)
        {
            winnerCountMap.TryGetValue(dto.Id, out var count);
            dto.WinnerCount = count;
        }

        return dtos;
    }

    public async Task<bool> AddPrizeAsync(LotteryPrizeDto dto)
    {
        if (await GetOwnedActivityAsync(dto.ActivityId) == null)
        {
            return false;
        }

        var entity = mapper.Map<LotteryPrizeEntity>(dto);
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();
        entity.Quota = Math.Max(dto.Quota, 1);
        return await lotteryPrizeRepository.AddAsync(entity);
    }

    public async Task<bool> UpdatePrizeAsync(LotteryPrizeUpdateDto dto)
    {
        var existing = await GetOwnedPrizeAsync(dto.Id);
        if (existing == null || existing.ActivityId != dto.ActivityId)
        {
            return false;
        }

        return await lotteryPrizeRepository.UpdateAsync(new LotteryPrizeEntity
        {
            Id = dto.Id,
            Name = dto.Name.Trim(),
            Quota = Math.Max(dto.Quota, 1),
            Description = dto.Description?.Trim(),
            State = dto.State,
            Sort = dto.Sort
        }, entity => new { entity.Name, entity.Quota, entity.Description, entity.State, entity.Sort }) > 0;
    }

    public async Task<bool> DeletePrizeAsync(long id)
    {
        var prize = await GetOwnedPrizeAsync(id);
        if (prize == null)
        {
            return false;
        }

        return await lotteryPrizeRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            // 奖项删除后，历史中奖记录同步标记删除，避免结果页出现孤立奖项。
            await lotteryWinnerRepository.DeleteAsync(entity => entity.PrizeId == id && entity.TenantId == TenantContextHolder.TenantId, transaction);
            await lotteryPrizeRepository.DeleteByIdAsync(id, transaction);
            return true;
        });
    }

    public async Task<List<LotteryParticipantDto>> GetParticipantListAsync(long activityId)
    {
        if (await GetOwnedActivityAsync(activityId) == null)
        {
            return new List<LotteryParticipantDto>();
        }

        var orderBy = OrderByConditionBuilder<LotteryParticipantEntity>.Build(OrderByType.Asc, entity => entity.Sort,
            OrderByConditionBuilder<LotteryParticipantEntity>.Build(OrderByType.Asc, entity => entity.CreateTime));
        var entities = (await lotteryParticipantRepository.QueryAsync(entity =>
            entity.ActivityId == activityId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete,
            orderBy))?.ToList() ?? new List<LotteryParticipantEntity>();

        return mapper.Map<List<LotteryParticipantDto>>(entities);
    }

    public async Task<bool> AddParticipantAsync(LotteryParticipantDto dto)
    {
        if (await GetOwnedActivityAsync(dto.ActivityId) == null)
        {
            return false;
        }

        var entity = mapper.Map<LotteryParticipantEntity>(dto);
        entity.Name = dto.Name.Trim();
        entity.Code = dto.Code?.Trim();
        entity.Description = dto.Description?.Trim();
        return await lotteryParticipantRepository.AddAsync(entity);
    }

    public async Task<bool> UpdateParticipantAsync(LotteryParticipantUpdateDto dto)
    {
        var existing = await GetOwnedParticipantAsync(dto.Id);
        if (existing == null || existing.ActivityId != dto.ActivityId)
        {
            return false;
        }

        return await lotteryParticipantRepository.UpdateAsync(new LotteryParticipantEntity
        {
            Id = dto.Id,
            Name = dto.Name.Trim(),
            Code = dto.Code?.Trim(),
            Description = dto.Description?.Trim(),
            State = dto.State,
            Sort = dto.Sort
        }, entity => new { entity.Name, entity.Code, entity.Description, entity.State, entity.Sort }) > 0;
    }

    public async Task<bool> DeleteParticipantAsync(long id)
    {
        return await GetOwnedParticipantAsync(id) != null && await lotteryParticipantRepository.DeleteByIdAsync(id);
    }

    public async Task<int> ImportParticipantsAsync(LotteryParticipantImportReqDto dto)
    {
        if (await GetOwnedActivityAsync(dto.ActivityId) == null || string.IsNullOrWhiteSpace(dto.Content))
        {
            return 0;
        }

        var names = dto.Content
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        if (names.Count == 0)
        {
            return 0;
        }

        var entities = names.Select((name, index) => new LotteryParticipantEntity
        {
            ActivityId = dto.ActivityId,
            Name = name,
            State = CommonState.Enable,
            Sort = index
        }).ToList();

        return await lotteryParticipantRepository.AddAsync(entities) ? entities.Count : 0;
    }

    public async Task<List<LotteryWinnerDto>> DrawAsync(LotteryDrawReqDto dto)
    {
        var activity = await GetOwnedActivityAsync(dto.ActivityId);
        var prize = await GetOwnedPrizeAsync(dto.PrizeId);
        if (activity == null || prize == null || prize.ActivityId != dto.ActivityId || activity.State != CommonState.Enable || prize.State != CommonState.Enable)
        {
            return new List<LotteryWinnerDto>();
        }

        var participants = (await lotteryParticipantRepository.QueryAsync(entity =>
            entity.ActivityId == dto.ActivityId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<LotteryParticipantEntity>();
        var winners = (await lotteryWinnerRepository.QueryAsync(entity =>
            entity.ActivityId == dto.ActivityId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete))?.ToList() ?? new List<LotteryWinnerEntity>();
        // 候选人过滤同时受活动重复中奖配置和当前奖项已中奖人数影响。
        var candidates = LotteryDrawSelector.SelectCandidates(participants, winners, activity.AllowRepeatWinner, dto.PrizeId);
        var currentPrizeWinnerCount = winners.Count(entity => entity.PrizeId == dto.PrizeId);
        var drawCount = LotteryDrawSelector.CalculateDrawCount(dto.Count, prize.Quota, currentPrizeWinnerCount, candidates.Count);

        if (drawCount == 0)
        {
            return new List<LotteryWinnerDto>();
        }

        var batchNo = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var selectedParticipants = LotteryDrawSelector.Draw(candidates, drawCount);
        var winnerEntities = selectedParticipants.Select(participant => new LotteryWinnerEntity
        {
            ActivityId = dto.ActivityId,
            PrizeId = dto.PrizeId,
            ParticipantId = participant.Id,
            BatchNo = batchNo,
            // 中奖记录保留快照，后续修改奖项或参与人名称不会改变历史结果。
            WinnerNameSnapshot = participant.Name,
            PrizeNameSnapshot = prize.Name
        }).ToList();

        await lotteryWinnerRepository.AddAsync(winnerEntities);

        return mapper.Map<List<LotteryWinnerDto>>(winnerEntities);
    }

    public async Task<List<LotteryWinnerDto>> GetWinnerListAsync(long activityId)
    {
        if (await GetOwnedActivityAsync(activityId) == null)
        {
            return new List<LotteryWinnerDto>();
        }

        var orderBy = OrderByConditionBuilder<LotteryWinnerEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        var entities = (await lotteryWinnerRepository.QueryAsync(entity =>
            entity.ActivityId == activityId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete,
            orderBy))?.ToList() ?? new List<LotteryWinnerEntity>();

        return mapper.Map<List<LotteryWinnerDto>>(entities);
    }

    public async Task<bool> DeleteWinnerAsync(long id)
    {
        var winner = await lotteryWinnerRepository.GetByIdAsync(id);
        if (winner == null || winner.TenantId != TenantContextHolder.TenantId || await GetOwnedActivityAsync(winner.ActivityId) == null)
        {
            return false;
        }

        return await lotteryWinnerRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> ClearWinnersAsync(long activityId)
    {
        return await GetOwnedActivityAsync(activityId) != null &&
               await lotteryWinnerRepository.DeleteAsync(entity => entity.ActivityId == activityId && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }

    private async Task<LotteryActivityEntity?> GetOwnedActivityAsync(long activityId)
    {
        return await lotteryActivityRepository.GetAsync(entity =>
            entity.Id == activityId &&
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete);
    }

    private async Task<LotteryPrizeEntity?> GetOwnedPrizeAsync(long prizeId)
    {
        var prize = await lotteryPrizeRepository.GetAsync(entity =>
            entity.Id == prizeId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete);
        if (prize == null)
        {
            return null;
        }

        return await GetOwnedActivityAsync(prize.ActivityId) == null ? null : prize;
    }

    private async Task<LotteryParticipantEntity?> GetOwnedParticipantAsync(long participantId)
    {
        var participant = await lotteryParticipantRepository.GetAsync(entity =>
            entity.Id == participantId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete);
        if (participant == null)
        {
            return null;
        }

        return await GetOwnedActivityAsync(participant.ActivityId) == null ? null : participant;
    }
}
