using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 抽奖服务接口
/// </summary>
public interface ILotteryService
{
    Task<List<LotteryActivityDto>> GetActivityListAsync();

    Task<LotteryActivityDetailDto> GetActivityDetailAsync(long activityId);

    Task<bool> AddActivityAsync(LotteryActivityDto dto);

    Task<bool> UpdateActivityAsync(LotteryActivityUpdateDto dto);

    Task<bool> DeleteActivityAsync(long id);

    Task<List<LotteryPrizeDto>> GetPrizeListAsync(long activityId);

    Task<bool> AddPrizeAsync(LotteryPrizeDto dto);

    Task<bool> UpdatePrizeAsync(LotteryPrizeUpdateDto dto);

    Task<bool> DeletePrizeAsync(long id);

    Task<List<LotteryParticipantDto>> GetParticipantListAsync(long activityId);

    Task<bool> AddParticipantAsync(LotteryParticipantDto dto);

    Task<bool> UpdateParticipantAsync(LotteryParticipantUpdateDto dto);

    Task<bool> DeleteParticipantAsync(long id);

    Task<int> ImportParticipantsAsync(LotteryParticipantImportReqDto dto);

    Task<List<LotteryWinnerDto>> DrawAsync(LotteryDrawReqDto dto);

    Task<List<LotteryWinnerDto>> GetWinnerListAsync(long activityId);

    Task<bool> DeleteWinnerAsync(long id);

    Task<bool> ClearWinnersAsync(long activityId);
}
