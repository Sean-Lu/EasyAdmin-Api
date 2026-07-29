using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// AI用量记录器
/// </summary>
public interface IAiUsageRecorder
{
    /// <summary>
    /// 创建用量记录
    /// </summary>
    Task<AiUsageEntity> StartAsync(long conversationId, long messageId, string model);

    /// <summary>
    /// 完成用量记录
    /// </summary>
    Task CompleteAsync(
        long usageId,
        AiUsageStatus status,
        int inputTokens,
        int outputTokens,
        long durationMs,
        string? errorType);
}