using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 股票持仓管理
/// </summary>
public class StockHoldingController(
    IStockHoldingService stockHoldingService
    ) : BaseApiController
{
    /// <summary>
    /// 新增持仓
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(StockHoldingDto data)
    {
        return Success(await stockHoldingService.AddAsync(data));
    }

    /// <summary>
    /// 删除持仓
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var accountId = data?["accountId"]?.Value<long>() ?? default;
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await stockHoldingService.DeleteByIdAsync(accountId, id));
    }

    /// <summary>
    /// 修改持仓
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(StockHoldingUpdateDto data)
    {
        return Success(await stockHoldingService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改当前价格
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateCurrentPrice([FromBody] JObject? data)
    {
        var accountId = data?["accountId"]?.Value<long>() ?? default;
        var id = data?["id"]?.Value<long>() ?? default;
        var currentPrice = data?["currentPrice"]?.Value<decimal>() ?? default;
        return Success(await stockHoldingService.UpdateCurrentPriceAsync(accountId, id, currentPrice));
    }

    /// <summary>
    /// 获取持仓列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<StockHoldingListDto>> List(long accountId, string? keyword)
    {
        return Success(await stockHoldingService.ListAsync(accountId, keyword));
    }
}
