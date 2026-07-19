using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Sean.Core.Redis;
using EasyAdmin.Infrastructure.Wrapper;
using StackExchange.Redis;

namespace EasyAdmin.Application.Services;

/// <summary>
/// Redis缓存管理服务
/// </summary>
public class RedisCacheService(IConfiguration configuration) : IRedisCacheService
{
    private const int DefaultDatabaseCount = 16;
    private const int MaxPageSize = 200;

    public async Task<RedisServerInfoDto> GetServerInfoAsync(int database = 0)
    {
        var databaseIndex = ValidateDatabase(database);
        var keyCount = await RedisHelper.ExecuteAsync(database => database.ExecuteAsync("DBSIZE"), databaseIndex);
        var memoryInfo = await RedisHelper.ExecuteAsync(database => database.ExecuteAsync("INFO", "memory"), databaseIndex);
        var memory = memoryInfo.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(item => item.StartsWith("used_memory_human:", StringComparison.OrdinalIgnoreCase))?
            .Split(':', 2).ElementAtOrDefault(1)?.Trim();

        return new RedisServerInfoDto
        {
            Connected = true,
            EndPoints = configuration["Redis:EndPoints"],
            Database = databaseIndex,
            DatabaseCount = GetDatabaseCount(),
            KeyCount = (long)keyCount,
            UsedMemory = memory
        };
    }

    public async Task<ApiResultPageData<RedisCacheKeyDto>> PageKeysAsync(RedisCachePageReqDto request)
    {
        var databaseIndex = ValidateDatabase(request.Database);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var keys = SortKeys(await ScanKeysAsync(string.IsNullOrWhiteSpace(request.Pattern) ? "*" : request.Pattern.Trim(), databaseIndex));
        var pageKeys = keys.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var list = new List<RedisCacheKeyDto>();

        foreach (var key in pageKeys)
        {
            list.Add(await GetKeyInfoAsync(key, databaseIndex));
        }

        return new ApiResultPageData<RedisCacheKeyDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Total = keys.Count,
            List = list
        };
    }

    private static List<string> SortKeys(IEnumerable<string> keys) =>
        keys.Order(StringComparer.Ordinal).ToList();

    private static int NormalizeDatabase(int database, int databaseCount)
    {
        if (database < 0 || database >= databaseCount) throw new ArgumentOutOfRangeException(nameof(database));
        return database;
    }

    public async Task<RedisCacheDetailDto?> GetDetailAsync(string key, int database = 0)
    {
        var databaseIndex = ValidateDatabase(database);
        if (!await RedisHelper.ExecuteAsync(db => db.KeyExistsAsync(key), databaseIndex)) return null;

        var info = await GetKeyInfoAsync(key, databaseIndex);
        var value = await RedisHelper.ExecuteAsync(db => ReadValueAsync(db, key, info.Type), databaseIndex);
        return new RedisCacheDetailDto
        {
            Key = info.Key,
            Type = info.Type,
            TtlSeconds = info.TtlSeconds,
            Value = value
        };
    }

    public Task<bool> DeleteAsync(string key, int database = 0)
    {
        return RedisHelper.ExecuteAsync(db => db.KeyDeleteAsync(key), ValidateDatabase(database));
    }

    public async Task<long> DeleteByPatternAsync(string pattern, int database = 0)
    {
        var databaseIndex = ValidateDatabase(database);
        var keys = await ScanKeysAsync(pattern, databaseIndex);
        return keys.Count == 0 ? 0 : await RedisHelper.ExecuteAsync(db => db.KeyDeleteAsync(keys.Select(key => (RedisKey)key).ToArray()), databaseIndex);
    }

    public async Task ClearDatabaseAsync(int database = 0)
    {
        await RedisHelper.ExecuteAsync(db => db.ExecuteAsync("FLUSHDB"), ValidateDatabase(database));
    }

    private async Task<List<string>> ScanKeysAsync(string pattern, int databaseIndex)
    {
        var keys = new List<string>();
        long cursor = 0;
        do
        {
            var result = await RedisHelper.ExecuteAsync(database => database.ExecuteAsync("SCAN", cursor, "MATCH", pattern, "COUNT", 200), databaseIndex);
            var parts = (RedisResult[])result;
            cursor = (long)parts[0];
            keys.AddRange(((RedisResult[])parts[1]).Select(item => (string)item));
        } while (cursor != 0);

        return keys;
    }

    private static async Task<RedisCacheKeyDto> GetKeyInfoAsync(string key, int databaseIndex)
    {
        var type = await RedisHelper.ExecuteAsync(database => database.KeyTypeAsync(key), databaseIndex);
        var ttl = await RedisHelper.ExecuteAsync(database => database.KeyTimeToLiveAsync(key), databaseIndex);
        return new RedisCacheKeyDto
        {
            Key = key,
            Type = type.ToString().ToLowerInvariant(),
            TtlSeconds = ttl.HasValue ? (long)ttl.Value.TotalSeconds : -1
        };
    }

    private int GetDatabaseCount() => Math.Max(1, configuration.GetValue("Redis:DatabaseCount", DefaultDatabaseCount));

    private int ValidateDatabase(int database)
    {
        try
        {
            return NormalizeDatabase(database, GetDatabaseCount());
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ExplicitException($"Redis数据库索引必须在 0 到 {GetDatabaseCount() - 1} 之间");
        }
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
