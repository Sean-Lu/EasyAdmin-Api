using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 股票账户管理
/// </summary>
public class StockAccountController(
    IStockAccountService stockAccountService
    ) : BaseApiController
{
    /// <summary>
    /// 新增账户
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(StockAccountDto data)
    {
        return Success(await stockAccountService.AddAsync(data));
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await stockAccountService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改账户
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(StockAccountUpdateDto data)
    {
        return Success(await stockAccountService.UpdateAsync(data));
    }

    /// <summary>
    /// 获取账户列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<StockAccountDto>>> List()
    {
        return Success(await stockAccountService.ListAsync());
    }
}
