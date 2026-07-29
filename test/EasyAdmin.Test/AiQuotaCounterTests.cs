using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Wrapper;
using Sean.Core.Redis;

namespace EasyAdmin.Test;

[TestClass]
public class AiQuotaCounterTests
{
    private readonly List<string> cacheKeys = [];

    [TestInitialize]
    public void Initialize()
    {
        RedisTestSetup.EnsureInitialized();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        foreach (var cacheKey in cacheKeys)
        {
            await RedisHelper.KeyDeleteAsync(cacheKey);
        }
    }

    [TestMethod]
    public async Task ReserveAsync_FirstReservationUsesTenantAndLocalDateKey()
    {
        var now = new DateTime(2030, 3, 4, 23, 59, 59, DateTimeKind.Unspecified);
        var tenantId = NextTenantId();
        TrackKey(tenantId, now);
        var counter = new AiQuotaCounter();

        await counter.ReserveAsync(tenantId, 10, now);

        Assert.AreEqual(1, await counter.GetUsedAsync(tenantId, now));
        Assert.AreEqual(0, await counter.GetUsedAsync(tenantId, now.AddDays(1)));
    }

    [TestMethod]
    public async Task ReserveAsync_ConcurrentCallsAreAtomicAndOverLimitAttemptsRemainCounted()
    {
        var now = new DateTime(2030, 6, 1, 8, 0, 0, DateTimeKind.Unspecified);
        var tenantId = NextTenantId();
        TrackKey(tenantId, now);
        var counter = new AiQuotaCounter();

        var reservations = Enumerable.Range(0, 20)
            .Select(async _ =>
            {
                try
                {
                    await counter.ReserveAsync(tenantId, 5, now);
                    return true;
                }
                catch (ExplicitException)
                {
                    return false;
                }
            });

        var results = await Task.WhenAll(reservations);

        Assert.AreEqual(5, results.Count(result => result));
        Assert.AreEqual(15, results.Count(result => !result));
        Assert.AreEqual(20, await counter.GetUsedAsync(tenantId, now));
    }

    [TestMethod]
    public async Task ReserveAsync_FirstReservationExpiresAfterNextLocalMidnightGracePeriod()
    {
        var now = DateTime.Now;
        var tenantId = NextTenantId();
        var key = TrackKey(tenantId, now);
        var counter = new AiQuotaCounter();

        await counter.ReserveAsync(tenantId, 10, now);

        var ttl = await RedisHelper.ExecuteAsync(database => database.KeyTimeToLiveAsync(key));
        var expected = now.Date.AddDays(1).AddMinutes(5) - now;
        Assert.IsNotNull(ttl);
        Assert.IsTrue(ttl <= expected + TimeSpan.FromSeconds(1));
        Assert.IsTrue(ttl >= expected - TimeSpan.FromSeconds(5));
    }

    private static long NextTenantId()
    {
        return DateTime.UtcNow.Ticks ^ Random.Shared.NextInt64(1, long.MaxValue);
    }

    private string TrackKey(long tenantId, DateTime now)
    {
        var key = $"{CacheKeyConst.AiTenantDailyQuotaPrefix}{tenantId}:{DateOnly.FromDateTime(now):yyyyMMdd}";
        cacheKeys.Add(key);
        return key;
    }
}