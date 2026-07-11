using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Sean.Core.Redis;
using StackExchange.Redis;

namespace EasyAdmin.Application.Services;

/// <summary>
/// Redis缓存管理服务
/// </summary>
public class RedisCacheService(IConfiguration configuration) : IRedisCacheService
{
    private const int DatabaseIndex = 0;
    private const int MaxPageSize = 200;

    public async Task<RedisServerInfoDto> GetServerInfoAsync()
    {
        var keyCount = await RedisHelper.ExecuteAsync(database => database.ExecuteAsync("DBSIZE"), DatabaseIndex);
        var memoryInfo = await RedisHelper.ExecuteAsync(database => database.ExecuteAsync("INFO", "memory"), DatabaseIndex);
        var memory = memoryInfo.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(item => item.StartsWith("used_memory_human:", StringComparison.OrdinalIgnoreCase))?
            .Split(':', 2).ElementAtOrDefault(1)?.Trim();

        return new RedisServerInfoDto
        {
            Connected = true,
            EndPoints = configuration["Redis:EndPoints"],
            Database = DatabaseIndex,
            KeyCount = (long)keyCount,
            UsedMemory = memory
        };
    }

    public async Task<ApiResultPageData<RedisCacheKeyDto>> PageKeysAsync(RedisCachePageReqDto request)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var keys = await ScanKeysAsync(string.IsNullOrWhiteSpace(request.Pattern) ? "*" : request.Pattern.Trim());
        var pageKeys = keys.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var list = new List<RedisCacheKeyDto>();

        foreach (var key in pageKeys)
        {
            list.Add(await GetKeyInfoAsync(key));
        }

        return new ApiResultPageData<RedisCacheKeyDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Total = keys.Count,
            List = list
        };
    }

    public async Task<RedisCacheDetailDto?> GetDetailAsync(string key)
    {
        if (!await RedisHelper.KeyExistsAsync(key)) return null;

        var info = await GetKeyInfoAsync(key);
        var value = await RedisHelper.ExecuteAsync(database => ReadValueAsync(database, key, info.Type), DatabaseIndex);
        return new RedisCacheDetailDto
        {
            Key = info.Key,
            Type = info.Type,
            TtlSeconds = info.TtlSeconds,
            Value = value
        };
    }

    public Task<bool> DeleteAsync(string key)
    {
        return RedisHelper.KeyDeleteAsync(key);
    }

    public async Task<long> DeleteByPatternAsync(string pattern)
    {
        var keys = await ScanKeysAsync(pattern);
        return keys.Count == 0 ? 0 : await RedisHelper.KeyDeleteAsync(keys);
    }

    public async Task ClearDatabaseAsync()
    {
        await RedisHelper.ExecuteAsync(database => database.ExecuteAsync("FLUSHDB"), DatabaseIndex);
    }

    private static async Task<List<string>> ScanKeysAsync(string pattern)
    {
        var keys = new List<string>();
        long cursor = 0;
        do
        {
            var result = await RedisHelper.ExecuteAsync(database => database.ExecuteAsync("SCAN", cursor, "MATCH", pattern, "COUNT", 200), DatabaseIndex);
            var parts = (RedisResult[])result;
            cursor = (long)parts[0];
            keys.AddRange(((RedisResult[])parts[1]).Select(item => (string)item));
        } while (cursor != 0);

        return keys;
    }

    private static async Task<RedisCacheKeyDto> GetKeyInfoAsync(string key)
    {
        var type = await RedisHelper.ExecuteAsync(database => database.KeyTypeAsync(key), DatabaseIndex);
        var ttl = await RedisHelper.KeyTimeToLiveAsync(key);
        return new RedisCacheKeyDto
        {
            Key = key,
            Type = type.ToString().ToLowerInvariant(),
            TtlSeconds = ttl.HasValue ? (long)ttl.Value.TotalSeconds : -1
        };
    }

    private static async Task<string> ReadValueAsync(IDatabase database, string key, string type)
    {
        object value = type switch
        {
            "string" => await database.StringGetAsync(key),
            "list" => await database.ListRangeAsync(key, 0, -1),
            "set" => await database.SetMembersAsync(key),
            "hash" => await database.HashGetAllAsync(key),
            "zset" => await database.SortedSetRangeByRankWithScoresAsync(key),
            _ => await database.ExecuteAsync("GET", key)
        };
        return value is RedisValue redisValue ? redisValue.ToString() : JsonConvert.SerializeObject(value);
    }
}
