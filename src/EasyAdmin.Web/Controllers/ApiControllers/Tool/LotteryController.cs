using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 抽奖工具
/// </summary>
public class LotteryController(
    ILogger<LotteryController> logger,
    ILotteryService lotteryService
    ) : BaseApiController
{
    /// <summary>
    /// 获取当前用户创建的抽奖活动列表。
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<LotteryActivityDto>>> ActivityList()
    {
        return Success(await lotteryService.GetActivityListAsync());
    }

    /// <summary>
    /// 获取活动详情，包含奖项、参与人和中奖记录。
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<LotteryActivityDetailDto>> ActivityDetail(long activityId)
    {
        return Success(await lotteryService.GetActivityDetailAsync(activityId));
    }

    /// <summary>
    /// 创建抽奖活动。
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> ActivityAdd(LotteryActivityDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("活动名称不能为空");
        }

        return Success(await lotteryService.AddActivityAsync(data));
    }

    /// <summary>
    /// 更新抽奖活动基础信息和重复中奖规则。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> ActivityUpdate(LotteryActivityUpdateDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("活动名称不能为空");
        }

        return Success(await lotteryService.UpdateActivityAsync(data));
    }

    /// <summary>
    /// 删除抽奖活动，并同步逻辑删除活动下的奖项、参与人和中奖记录。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> ActivityDelete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await lotteryService.DeleteActivityAsync(id));
    }

    /// <summary>
    /// 获取活动下的奖项列表。
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<LotteryPrizeDto>>> PrizeList(long activityId)
    {
        return Success(await lotteryService.GetPrizeListAsync(activityId));
    }

    /// <summary>
    /// 新增奖项，中奖名额必须大于 0。
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> PrizeAdd(LotteryPrizeDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("奖项名称不能为空");
        }

        if (data.Quota < 1)
        {
            return Fail<bool>("中奖名额必须大于 0");
        }

        return Success(await lotteryService.AddPrizeAsync(data));
    }

    /// <summary>
    /// 更新奖项名称、名额、排序和启用状态。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> PrizeUpdate(LotteryPrizeUpdateDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("奖项名称不能为空");
        }

        if (data.Quota < 1)
        {
            return Fail<bool>("中奖名额必须大于 0");
        }

        return Success(await lotteryService.UpdatePrizeAsync(data));
    }

    /// <summary>
    /// 删除奖项，并同步逻辑删除该奖项的中奖记录。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> PrizeDelete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await lotteryService.DeletePrizeAsync(id));
    }

    /// <summary>
    /// 获取活动下的参与人列表。
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<LotteryParticipantDto>>> ParticipantList(long activityId)
    {
        return Success(await lotteryService.GetParticipantListAsync(activityId));
    }

    /// <summary>
    /// 新增单个参与人。
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> ParticipantAdd(LotteryParticipantDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("参与人姓名不能为空");
        }

        return Success(await lotteryService.AddParticipantAsync(data));
    }

    /// <summary>
    /// 更新参与人姓名、编号、备注、排序和启用状态。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> ParticipantUpdate(LotteryParticipantUpdateDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("参与人姓名不能为空");
        }

        return Success(await lotteryService.UpdateParticipantAsync(data));
    }

    /// <summary>
    /// 删除单个参与人。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> ParticipantDelete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await lotteryService.DeleteParticipantAsync(id));
    }

    /// <summary>
    /// 批量导入参与人，每行作为一个参与人姓名。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<int>> ParticipantImport(LotteryParticipantImportReqDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Content))
        {
            return Fail<int>("导入内容不能为空");
        }

        return Success(await lotteryService.ImportParticipantsAsync(data));
    }

    /// <summary>
    /// 停止现场抽奖并生成中奖记录。
    /// 前端“开始抽奖”只播放滚动动画，只有调用本接口时才会按规则抽取并持久化中奖人。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<List<LotteryWinnerDto>>> Draw(LotteryDrawReqDto data)
    {
        return Success(await lotteryService.DrawAsync(data));
    }

    /// <summary>
    /// 获取活动下的中奖记录。
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<LotteryWinnerDto>>> WinnerList(long activityId)
    {
        return Success(await lotteryService.GetWinnerListAsync(activityId));
    }

    /// <summary>
    /// 删除单条中奖记录，可用于人工回退某次中奖结果。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> WinnerDelete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await lotteryService.DeleteWinnerAsync(id));
    }

    /// <summary>
    /// 清空指定活动的全部中奖记录。
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> ClearWinners([FromBody] JObject? data)
    {
        var activityId = data?["activityId"]?.Value<long>() ?? default;
        return Success(await lotteryService.ClearWinnersAsync(activityId));
    }
}
