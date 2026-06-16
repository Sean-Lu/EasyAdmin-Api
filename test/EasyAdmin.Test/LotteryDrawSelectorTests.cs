using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Test;

[TestClass]
public class LotteryDrawSelectorTests
{
    private static List<LotteryParticipantEntity> Participants() => new()
    {
        new LotteryParticipantEntity { Id = 1, ActivityId = 10, Name = "A", State = CommonState.Enable },
        new LotteryParticipantEntity { Id = 2, ActivityId = 10, Name = "B", State = CommonState.Enable },
        new LotteryParticipantEntity { Id = 3, ActivityId = 10, Name = "C", State = CommonState.Enable },
        new LotteryParticipantEntity { Id = 4, ActivityId = 10, Name = "D", State = CommonState.Disable }
    };

    [TestMethod]
    public void SelectCandidates_Excludes_All_Activity_Winners_When_Repeat_Disallowed()
    {
        var winners = new List<LotteryWinnerEntity>
        {
            new LotteryWinnerEntity { ActivityId = 10, PrizeId = 100, ParticipantId = 1 }
        };

        var candidates = LotteryDrawSelector.SelectCandidates(Participants(), winners, allowRepeatWinner: false, prizeId: 101);

        CollectionAssert.AreEquivalent(new List<long> { 2, 3 }, candidates.Select(c => c.Id).ToList());
    }

    [TestMethod]
    public void SelectCandidates_Only_Excludes_Current_Prize_Winners_When_Repeat_Allowed()
    {
        var winners = new List<LotteryWinnerEntity>
        {
            new LotteryWinnerEntity { ActivityId = 10, PrizeId = 100, ParticipantId = 1 },
            new LotteryWinnerEntity { ActivityId = 10, PrizeId = 101, ParticipantId = 2 }
        };

        var candidates = LotteryDrawSelector.SelectCandidates(Participants(), winners, allowRepeatWinner: true, prizeId: 101);

        CollectionAssert.AreEquivalent(new List<long> { 1, 3 }, candidates.Select(c => c.Id).ToList());
    }

    [TestMethod]
    public void CalculateDrawCount_Uses_Minimum_Of_Request_Remaining_Quota_And_Candidates()
    {
        var count = LotteryDrawSelector.CalculateDrawCount(requestCount: 5, prizeQuota: 3, currentPrizeWinnerCount: 1, candidateCount: 10);

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public void CalculateDrawCount_Returns_Zero_When_Prize_Is_Full()
    {
        var count = LotteryDrawSelector.CalculateDrawCount(requestCount: 1, prizeQuota: 2, currentPrizeWinnerCount: 2, candidateCount: 3);

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void Draw_Takes_Requested_Count_Without_Duplicates()
    {
        var drawn = LotteryDrawSelector.Draw(Participants().Where(p => p.State == CommonState.Enable).ToList(), 2);

        Assert.AreEqual(2, drawn.Count);
        Assert.AreEqual(2, drawn.Select(p => p.Id).Distinct().Count());
    }
}
