using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Services;

public static class LotteryDrawSelector
{
    public static List<LotteryParticipantEntity> SelectCandidates(
        IEnumerable<LotteryParticipantEntity> participants,
        IEnumerable<LotteryWinnerEntity> winners,
        bool allowRepeatWinner,
        long prizeId)
    {
        var excludedIds = allowRepeatWinner
            ? winners.Where(winner => winner.PrizeId == prizeId).Select(winner => winner.ParticipantId).ToHashSet()
            : winners.Select(winner => winner.ParticipantId).ToHashSet();

        return participants
            .Where(participant => participant.State == CommonState.Enable)
            .Where(participant => !excludedIds.Contains(participant.Id))
            .ToList();
    }

    public static int CalculateDrawCount(int requestCount, int prizeQuota, int currentPrizeWinnerCount, int candidateCount)
    {
        var normalizedRequestCount = Math.Max(requestCount, 1);
        var remainingQuota = Math.Max(prizeQuota - currentPrizeWinnerCount, 0);
        return Math.Min(normalizedRequestCount, Math.Min(remainingQuota, candidateCount));
    }

    public static List<LotteryParticipantEntity> Draw(IReadOnlyList<LotteryParticipantEntity> candidates, int count)
    {
        return candidates
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(Math.Max(count, 0), candidates.Count))
            .ToList();
    }
}
