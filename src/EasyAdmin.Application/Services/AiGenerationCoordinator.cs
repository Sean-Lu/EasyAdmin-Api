using EasyAdmin.Application.Contracts;
using EasyAdmin.Infrastructure.Const;
using Sean.Core.Redis;

namespace EasyAdmin.Application.Services;

/// <summary>
/// AI生成任务Redis协调实现
/// </summary>
public sealed class AiGenerationCoordinator : IAiGenerationCoordinator
{
    private static readonly TimeSpan LeaseExpiration = TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public async Task<string?> TryAcquireAsync(long tenantId, long userId, long conversationId)
    {
        var leaseToken = Guid.NewGuid().ToString("N");
        return await RedisHelper.LockTakeAsync(
            GetLockKey(tenantId, userId, conversationId),
            leaseToken,
            LeaseExpiration)
            ? leaseToken
            : null;
    }

    /// <inheritdoc />
    public async Task ReleaseAsync(long tenantId, long userId, long conversationId, string leaseToken)
    {
        var cancellationKey = GetCancellationKey(tenantId, userId, conversationId);
        var cancellationToken = await RedisHelper.StringGetAsync<string>(cancellationKey);
        if (string.Equals(cancellationToken, leaseToken, StringComparison.Ordinal))
        {
            await RedisHelper.KeyDeleteAsync(cancellationKey);
        }

        await RedisHelper.LockReleaseAsync(
            GetLockKey(tenantId, userId, conversationId),
            leaseToken);
    }

    /// <inheritdoc />
    public async Task<bool> RequestCancellationAsync(long tenantId, long userId, long conversationId)
    {
        var leaseToken = await RedisHelper.StringGetAsync<string>(
            GetLockKey(tenantId, userId, conversationId));
        if (string.IsNullOrWhiteSpace(leaseToken))
        {
            return false;
        }

        return await RedisHelper.StringSetAsync(
            GetCancellationKey(tenantId, userId, conversationId),
            leaseToken,
            LeaseExpiration);
    }

    /// <inheritdoc />
    public async Task<bool> IsCancellationRequestedAsync(
        long tenantId,
        long userId,
        long conversationId,
        string leaseToken)
    {
        var cancellationToken = await RedisHelper.StringGetAsync<string>(
            GetCancellationKey(tenantId, userId, conversationId));
        return string.Equals(cancellationToken, leaseToken, StringComparison.Ordinal);
    }

    private static string GetLockKey(long tenantId, long userId, long conversationId)
    {
        return $"{CacheKeyConst.AiConversationGenerationLockPrefix}{tenantId}:{userId}:{conversationId}";
    }

    private static string GetCancellationKey(long tenantId, long userId, long conversationId)
    {
        return $"{CacheKeyConst.AiConversationCancellationPrefix}{tenantId}:{userId}:{conversationId}";
    }
}
