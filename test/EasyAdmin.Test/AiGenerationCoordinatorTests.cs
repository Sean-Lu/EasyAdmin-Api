using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Const;
using Sean.Core.Redis;

namespace EasyAdmin.Test;

[TestClass]
public class AiGenerationCoordinatorTests
{
    private long conversationId;

    [TestInitialize]
    public void Initialize()
    {
        RedisTestSetup.EnsureInitialized();
        conversationId = DateTime.UtcNow.Ticks ^ Random.Shared.NextInt64(1, long.MaxValue);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await RedisHelper.KeyDeleteAsync(
            $"{CacheKeyConst.AiConversationGenerationLockPrefix}7:11:{conversationId}");
        await RedisHelper.KeyDeleteAsync(
            $"{CacheKeyConst.AiConversationCancellationPrefix}7:11:{conversationId}");
    }

    [TestMethod]
    public async Task LeaseAndCancellation_AreSharedAcrossCoordinatorInstances()
    {
        var first = new AiGenerationCoordinator();
        var second = new AiGenerationCoordinator();
        var leaseToken = await first.TryAcquireAsync(7, 11, conversationId);

        Assert.IsNotNull(leaseToken);
        try
        {
            Assert.IsNull(await second.TryAcquireAsync(7, 11, conversationId));
            Assert.IsTrue(await second.RequestCancellationAsync(7, 11, conversationId));
            Assert.IsTrue(await first.IsCancellationRequestedAsync(
                7,
                11,
                conversationId,
                leaseToken));
        }
        finally
        {
            await first.ReleaseAsync(7, 11, conversationId, leaseToken);
        }

        var nextLeaseToken = await second.TryAcquireAsync(7, 11, conversationId);
        Assert.IsNotNull(nextLeaseToken);
        try
        {
            Assert.IsFalse(await second.IsCancellationRequestedAsync(
                7,
                11,
                conversationId,
                nextLeaseToken));
        }
        finally
        {
            await second.ReleaseAsync(7, 11, conversationId, nextLeaseToken);
        }
    }
}
