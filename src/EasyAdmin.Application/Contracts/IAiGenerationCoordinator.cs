namespace EasyAdmin.Application.Contracts;

/// <summary>
/// AI生成任务协调接口
/// </summary>
public interface IAiGenerationCoordinator
{
    /// <summary>
    /// 尝试获取会话生成租约
    /// </summary>
    Task<string?> TryAcquireAsync(long tenantId, long userId, long conversationId);

    /// <summary>
    /// 释放会话生成租约
    /// </summary>
    Task ReleaseAsync(long tenantId, long userId, long conversationId, string leaseToken);

    /// <summary>
    /// 请求取消会话生成
    /// </summary>
    Task<bool> RequestCancellationAsync(long tenantId, long userId, long conversationId);

    /// <summary>
    /// 判断当前租约是否已收到取消请求
    /// </summary>
    Task<bool> IsCancellationRequestedAsync(
        long tenantId,
        long userId,
        long conversationId,
        string leaseToken);
}
