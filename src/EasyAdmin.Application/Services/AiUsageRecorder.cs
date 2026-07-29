using EasyAdmin.Application.Contracts;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <inheritdoc />
public sealed class AiUsageRecorder(IAiUsageRepository usageRepository) : IAiUsageRecorder
{
    /// <inheritdoc />
    public async Task<AiUsageEntity> StartAsync(long conversationId, long messageId, string model)
    {
        var entity = new AiUsageEntity
        {
            TenantId = TenantContextHolder.TenantId,
            UserId = TenantContextHolder.UserId,
            ConversationId = conversationId,
            MessageId = messageId,
            Model = NormalizeModel(model),
            Status = AiUsageStatus.Running
        };
        if (!await usageRepository.AddAsync(entity))
        {
            throw new ExplicitException("AI用量记录创建失败");
        }

        return entity;
    }

    /// <inheritdoc />
    public async Task CompleteAsync(
        long usageId,
        AiUsageStatus status,
        int inputTokens,
        int outputTokens,
        long durationMs,
        string? errorType)
    {
        if (status is not (AiUsageStatus.Succeeded or AiUsageStatus.Failed or AiUsageStatus.Cancelled))
        {
            throw new ExplicitException("AI用量状态无效");
        }

        var normalizedInputTokens = Math.Max(0, inputTokens);
        var normalizedOutputTokens = Math.Max(0, outputTokens);
        var totalTokens = Math.Min(
            (long)normalizedInputTokens + normalizedOutputTokens,
            int.MaxValue);
        var entity = new AiUsageEntity
        {
            Id = usageId,
            Status = status,
            InputTokens = normalizedInputTokens,
            OutputTokens = normalizedOutputTokens,
            TotalTokens = (int)totalTokens,
            DurationMs = (int)Math.Clamp(durationMs, 0, int.MaxValue),
            ErrorType = status == AiUsageStatus.Succeeded
                ? null
                : NormalizeErrorType(errorType)
        };
        var affected = await usageRepository.UpdateAsync(
            entity,
            item => new
            {
                item.Status,
                item.InputTokens,
                item.OutputTokens,
                item.TotalTokens,
                item.DurationMs,
                item.ErrorType
            },
            item =>
                item.Id == usageId &&
                item.TenantId == TenantContextHolder.TenantId &&
                item.UserId == TenantContextHolder.UserId &&
                item.Status == AiUsageStatus.Running &&
                !item.IsDelete);
        if (affected != 1)
        {
            throw new ExplicitException("AI用量记录不存在或不属于当前用户");
        }
    }

    private static string NormalizeModel(string model)
    {
        var normalized = model.Trim();
        return normalized.Length <= 100 ? normalized : normalized[..100];
    }

    private static string? NormalizeErrorType(string? errorType)
    {
        if (string.IsNullOrWhiteSpace(errorType))
        {
            return "ai_provider_error";
        }

        var normalized = errorType.Trim().ToLowerInvariant();
        return normalized switch
        {
            "ai_authentication" or
            "ai_rate_limited" or
            "ai_timeout" or
            "ai_cancelled" or
            "ai_invalid_response" or
            "ai_provider_error" or
            "ai_business_error" or
            "ai_tool_limit" => normalized,
            _ => "ai_provider_error"
        };
    }
}
