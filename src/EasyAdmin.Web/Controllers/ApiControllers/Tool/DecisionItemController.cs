using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 随机决策候选项管理
/// </summary>
public class DecisionItemController(
    ILogger<DecisionItemController> logger,
    IDecisionItemService decisionItemService
    ) : BaseApiController
{
    /// <summary>
    /// 新增候选项
    /// </summary>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(DecisionItemDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("候选项名称不能为空");
        }

        return Success(await decisionItemService.AddAsync(data));
    }

    /// <summary>
    /// 删除候选项
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        return Success(await decisionItemService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改候选项
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(DecisionItemUpdateDto data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return Fail<bool>("候选项名称不能为空");
        }

        return Success(await decisionItemService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改候选项状态
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data?["state"]?.Value<int>() ?? default);
        return Success(await decisionItemService.UpdateStateAsync(id, state));
    }

    /// <summary>
    /// 获取候选项列表
    /// </summary>
    [HttpGet]
    public async Task<ApiResult<List<DecisionItemDto>>> List(DecisionItemType type)
    {
        return Success(await decisionItemService.GetByUserIdAsync(type));
    }

    /// <summary>
    /// 随机抽取候选项
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<List<DecisionItemDto>>> Draw(DecisionDrawReqDto data)
    {
        return Success(await decisionItemService.DrawAsync(data));
    }
}
