using EasyAdmin.Application.Contracts;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Wrapper;
using Sean.Core.Redis;
using StackExchange.Redis;

namespace EasyAdmin.Application.Services;

/// <inheritdoc />
public sealed class AiQuotaCounter : IAiQuotaCounter
{
    private const string ReserveScript = """
        local current = redis.call('INCR', KEYS[1])
        if current == 1 then
            redis.call('EXPIRE', KEYS[1], ARGV[1])
        end
        return current
        """;

    /// <inheritdoc />
    public async Task ReserveAsync(long tenantId, int limit, DateTime now)
    {
        var localNow = ToLocalTime(now);
        var key = GetKey(tenantId, localNow);
        var expiresAt = localNow.Date.AddDays(1).AddMinutes(5);
        var expirySeconds = Math.Max(1L, (long)Math.Ceiling((expiresAt - localNow).TotalSeconds));
        var result = await RedisHelper.ExecuteAsync(database =>
            database.ScriptEvaluateAsync(
                ReserveScript,
                [(RedisKey)key],
                [(RedisValue)expirySeconds]));
        var used = (long)result;
        if (used > limit)
        {
            throw new ExplicitException("今日 AI 调用次数已用完");
        }
    }

    /// <inheritdoc />
    public Task<long> GetUsedAsync(long tenantId, DateTime now)
    {
        return RedisHelper.StringGetAsync<long>(GetKey(tenantId, ToLocalTime(now)));
    }

    private static DateTime ToLocalTime(DateTime now)
    {
        return now.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.Local)
            : now;
    }

    private static string GetKey(long tenantId, DateTime now)
    {
        return $"{CacheKeyConst.AiTenantDailyQuotaPrefix}{tenantId}:{DateOnly.FromDateTime(now):yyyyMMdd}";
    }
}