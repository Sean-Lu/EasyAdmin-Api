using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// Redis缓存管理
/// </summary>
public class RedisCacheController(
    ILogger<RedisCacheController> logger,
    IRedisCacheService redisCacheService
    ) : BaseApiController
{
    /// <summary>
    /// 获取Redis服务器信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<RedisServerInfoDto>> ServerInfo([FromQuery] int database = 0)
    {
        return Success(await redisCacheService.GetServerInfoAsync(database));
    }

    /// <summary>
    /// 分页查询Redis缓存键
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<RedisCacheKeyDto>>> Page([FromQuery] RedisCachePageReqDto request)
    {
        return Success(await redisCacheService.PageKeysAsync(request));
    }

    /// <summary>
    /// 获取Redis缓存键详情
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<RedisCacheDetailDto>> Detail(string key, [FromQuery] int database = 0)
    {
        var detail = await redisCacheService.GetDetailAsync(key, database);
        return detail == null ? Fail<RedisCacheDetailDto>("缓存Key不存在") : Success(detail);
    }

    /// <summary>
    /// 删除Redis缓存键
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var key = data?["key"]?.Value<string>();
        var database = data?["database"]?.Value<int>() ?? 0;
        if (string.IsNullOrWhiteSpace(key)) throw new ExplicitException("缓存Key不能为空");

        var result = await redisCacheService.DeleteAsync(key, database);
        logger.LogWarning("删除Redis缓存，用户ID：{UserId}，Key：{Key}，结果：{Result}", UserId, key, result);
        return Success(result);
    }

    /// <summary>
    /// 批量删除Redis缓存键
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<long>> DeleteByPattern([FromBody] JObject? data)
    {
        var pattern = data?["pattern"]?.Value<string>();
        var database = data?["database"]?.Value<int>() ?? 0;
        if (string.IsNullOrWhiteSpace(pattern)) throw new ExplicitException("缓存匹配模式不能为空");

        var result = await redisCacheService.DeleteByPatternAsync(pattern, database);
        logger.LogWarning("批量删除Redis缓存，用户ID：{UserId}，匹配模式：{Pattern}，数量：{Count}", UserId, pattern, result);
        return Success(result);
    }

    /// <summary>
    /// 清空Redis当前数据库
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> ClearDatabase([FromBody] JObject? data)
    {
        var database = data?["database"]?.Value<int>() ?? 0;
        await redisCacheService.ClearDatabaseAsync(database);
        logger.LogWarning("清空Redis当前数据库，用户ID：{UserId}", UserId);
        return Success(true);
    }
}
