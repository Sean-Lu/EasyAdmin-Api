using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Infrastructure.Ai;
using EasyAdmin.Infrastructure.Enums;
using System.Diagnostics;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

/// <summary>
/// AI管理服务实现
/// </summary>
public sealed class AiAdminService(
    IAiModelConfigRepository modelConfigRepository,
    IAiTenantSettingRepository tenantSettingRepository,
    IAiUsageRepository usageRepository,
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    AiApiKeyProtector apiKeyProtector,
    IAiModelClient modelClient,
    IAiUsageRecorder usageRecorder) : IAiAdminService
{
    /// <inheritdoc />
    public async Task<AiModelConfigDto> GetModelConfigAsync()
    {
        var entity = await GetModelConfigEntityAsync();
        return ToModelConfigDto(entity);
    }

    /// <inheritdoc />
    public async Task<AiModelConfigDto> UpdateModelConfigAsync(AiModelConfigUpdateDto request)
    {
        ValidateModelConfig(request);
        var entity = await GetModelConfigEntityAsync();
        if (entity == null)
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey))
            {
                throw new ExplicitException("请填写AI接口密钥");
            }

            entity = new AiModelConfigEntity();
            ApplyModelConfig(entity, request, request.ApiKey);
            await modelConfigRepository.AddAsync(entity);
        }
        else
        {
            ApplyModelConfig(entity, request, request.ApiKey);
            await modelConfigRepository.UpdateAsync(
                entity,
                item => new
                {
                    item.BaseUrl,
                    item.Model,
                    item.ApiKeyEncrypted,
                    item.TimeoutSeconds,
                    item.MaxOutputTokens,
                    item.Temperature
                },
                item => item.Id == entity.Id && !item.IsDelete);
        }

        return ToModelConfigDto(entity);
    }

    /// <inheritdoc />
    public async Task<AiTenantSettingDto> GetTenantSettingAsync(long tenantId)
    {
        var entity = await tenantSettingRepository.GetAsync(item =>
            item.TenantId == tenantId && !item.IsDelete);
        return entity == null
            ? new AiTenantSettingDto { TenantId = tenantId }
            : ToTenantSettingDto(entity);
    }

    /// <inheritdoc />
    public async Task<AiTenantSettingDto> UpdateTenantSettingAsync(AiTenantSettingUpdateDto request)
    {
        if (request.TenantId < 1)
        {
            throw new ExplicitException("请选择有效租户");
        }
        if (request.Enabled && request.DailyRequestLimit < 1)
        {
            throw new ExplicitException("启用AI时每日请求上限必须大于0");
        }
        if (request.DailyRequestLimit < 0)
        {
            throw new ExplicitException("每日请求上限不能小于0");
        }

        var tenant = await tenantRepository.GetAsync(item =>
            item.Id == request.TenantId && !item.IsDelete);
        if (tenant == null)
        {
            throw new ExplicitException("租户不存在");
        }

        var entity = await tenantSettingRepository.GetAsync(item =>
            item.TenantId == request.TenantId && !item.IsDelete);
        if (entity == null)
        {
            entity = new AiTenantSettingEntity
            {
                TenantId = request.TenantId,
                Enabled = request.Enabled,
                DailyRequestLimit = request.DailyRequestLimit
            };
            await tenantSettingRepository.AddAsync(entity);
        }
        else
        {
            entity.Enabled = request.Enabled;
            entity.DailyRequestLimit = request.DailyRequestLimit;
            await tenantSettingRepository.UpdateAsync(
                entity,
                item => new { item.Enabled, item.DailyRequestLimit },
                item => item.TenantId == request.TenantId && !item.IsDelete);
        }

        return ToTenantSettingDto(entity);
    }

    /// <inheritdoc />
    public async Task<PageQueryResult<AiUsageDto>> PageUsageAsync(AiUsagePageReqDto request)
    {
        var userKeyword = request.UserKeyword?.Trim();
        var matchingUserIds = string.IsNullOrWhiteSpace(userKeyword)
            ? new List<long>()
            : (await userRepository.QueryAsync(user =>
                    !user.IsDelete &&
                    (!request.TenantId.HasValue || user.TenantId == request.TenantId.Value) &&
                    (user.UserName.Contains(userKeyword) || user.NickName.Contains(userKeyword))))
                ?.Select(user => user.Id)
                .ToList() ?? new List<long>();
        var orderBy = OrderByConditionBuilder<AiUsageEntity>.Build(OrderByType.Desc, item => item.CreateTime);
        orderBy.Next = OrderByConditionBuilder<AiUsageEntity>.Build(OrderByType.Desc, item => item.Id);
        var page = await usageRepository.PageQueryAsync(
            WhereExpressionUtil.Create<AiUsageEntity>(item => !item.IsDelete)
                .AndAlsoIF(request.TenantId.HasValue, item => item.TenantId == request.TenantId.GetValueOrDefault())
                .AndAlsoIF(request.UserId.HasValue, item => item.UserId == request.UserId.GetValueOrDefault())
                .AndAlsoIF(!string.IsNullOrWhiteSpace(userKeyword), item => matchingUserIds.Contains(item.UserId))
                .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Model), item => item.Model == request.Model)
                .AndAlsoIF(request.Status.HasValue, item => item.Status == request.Status.GetValueOrDefault())
                .AndAlsoIF(request.StartTime.HasValue, item => item.CreateTime >= request.StartTime)
                .AndAlsoIF(request.EndTime.HasValue, item => item.CreateTime <= request.EndTime),
            orderBy,
            request.PageNumber,
            request.PageSize);

        return new PageQueryResult<AiUsageDto>
        {
            Total = page.Total,
            List = page.List.Select(ToUsageDto).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<AiRuntimeConfig> GetRuntimeConfigAsync(long tenantId)
    {
        var setting = await tenantSettingRepository.GetAsync(item =>
            item.TenantId == tenantId && !item.IsDelete);
        if (setting is not { Enabled: true })
        {
            throw new ExplicitException("当前租户未启用AI");
        }

        var config = await GetModelConfigEntityAsync();
        if (config == null ||
            string.IsNullOrWhiteSpace(config.BaseUrl) ||
            string.IsNullOrWhiteSpace(config.Model) ||
            string.IsNullOrWhiteSpace(config.ApiKeyEncrypted))
        {
            throw new ExplicitException("AI模型配置不完整");
        }

        return new AiRuntimeConfig
        {
            BaseUrl = config.BaseUrl,
            Model = config.Model,
            ApiKey = apiKeyProtector.Decrypt(config.ApiKeyEncrypted),
            TimeoutSeconds = config.TimeoutSeconds,
            MaxOutputTokens = config.MaxOutputTokens,
            Temperature = config.Temperature,
            DailyRequestLimit = setting.DailyRequestLimit
        };
    }

    /// <inheritdoc />
    public async Task<AiConnectionTestResultDto> TestConnectionAsync()
    {
        var config = await GetModelConfigEntityAsync();
        if (config == null ||
            string.IsNullOrWhiteSpace(config.BaseUrl) ||
            string.IsNullOrWhiteSpace(config.Model) ||
            string.IsNullOrWhiteSpace(config.ApiKeyEncrypted))
        {
            throw new ExplicitException("AI模型配置不完整");
        }

        var usage = await usageRecorder.StartAsync(0, 0, config.Model);
        var stopwatch = Stopwatch.StartNew();
        var text = new System.Text.StringBuilder();
        var inputTokens = 0;
        var outputTokens = 0;
        try
        {
            await foreach (var item in modelClient.StreamAsync(
                               new AiModelClientOptions
                               {
                                   BaseUrl = config.BaseUrl,
                                   ApiKey = apiKeyProtector.Decrypt(config.ApiKeyEncrypted),
                                   Model = config.Model,
                                   Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
                               },
                               new AiModelRequest
                               {
                                   Messages = [new AiModelMessage("user", "只回复 OK")],
                                   Tools = [],
                                   Temperature = config.Temperature,
                                   MaxOutputTokens = 16
                               },
                               CancellationToken.None))
            {
                switch (item)
                {
                    case AiModelTextDelta delta when text.Length < 200:
                        text.Append(delta.Text.AsSpan(0, Math.Min(delta.Text.Length, 200 - text.Length)));
                        break;
                    case AiModelUsageCompleted modelUsage:
                        inputTokens = modelUsage.InputTokens;
                        outputTokens = modelUsage.OutputTokens;
                        break;
                }
            }

            stopwatch.Stop();
            await usageRecorder.CompleteAsync(
                usage.Id,
                AiUsageStatus.Succeeded,
                inputTokens,
                outputTokens,
                stopwatch.ElapsedMilliseconds,
                null);
            return new AiConnectionTestResultDto
            {
                Success = true,
                Result = text.ToString().Trim(),
                LatencyMs = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue)
            };
        }
        catch (AiModelClientException exception)
        {
            stopwatch.Stop();
            await usageRecorder.CompleteAsync(
                usage.Id,
                AiUsageStatus.Failed,
                inputTokens,
                outputTokens,
                stopwatch.ElapsedMilliseconds,
                exception.ErrorType);
            return new AiConnectionTestResultDto
            {
                Success = false,
                Result = GetConnectionFailureMessage(exception.ErrorType),
                LatencyMs = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue),
                ErrorType = exception.ErrorType
            };
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            await usageRecorder.CompleteAsync(
                usage.Id,
                AiUsageStatus.Failed,
                inputTokens,
                outputTokens,
                stopwatch.ElapsedMilliseconds,
                exception is ExplicitException ? "ai_business_error" : "ai_provider_error");
            throw;
        }
    }

    private Task<AiModelConfigEntity> GetModelConfigEntityAsync()
    {
        return modelConfigRepository.GetAsync(item =>
            item.ConfigKey == "default" && !item.IsDelete);
    }

    private static void ValidateModelConfig(AiModelConfigUpdateDto request)
    {
        if (!Uri.TryCreate(request.BaseUrl?.Trim(), UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttps &&
             !(baseUri.Scheme == Uri.UriSchemeHttp && baseUri.IsLoopback)))
        {
            throw new ExplicitException("AI服务地址必须使用HTTPS，本机地址可使用HTTP");
        }
        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new ExplicitException("请填写AI模型");
        }
        if (request.TimeoutSeconds is < 5 or > 300)
        {
            throw new ExplicitException("超时时间必须在5到300秒之间");
        }
        if (request.MaxOutputTokens is < 128 or > 32768)
        {
            throw new ExplicitException("最大输出令牌数必须在128到32768之间");
        }
        if (request.Temperature is < 0 or > 2)
        {
            throw new ExplicitException("温度必须在0到2之间");
        }
    }

    private void ApplyModelConfig(
        AiModelConfigEntity entity,
        AiModelConfigUpdateDto request,
        string? apiKey)
    {
        entity.BaseUrl = request.BaseUrl.Trim().TrimEnd('/');
        entity.Model = request.Model.Trim();
        entity.TimeoutSeconds = request.TimeoutSeconds;
        entity.MaxOutputTokens = request.MaxOutputTokens;
        entity.Temperature = request.Temperature;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            entity.ApiKeyEncrypted = apiKeyProtector.Encrypt(apiKey.Trim());
        }
    }

    private AiModelConfigDto ToModelConfigDto(AiModelConfigEntity? entity)
    {
        if (entity == null)
        {
            return new AiModelConfigDto
            {
                TimeoutSeconds = 60,
                MaxOutputTokens = 4096,
                Temperature = 0.7m
            };
        }

        var hasApiKey = !string.IsNullOrWhiteSpace(entity.ApiKeyEncrypted);
        return new AiModelConfigDto
        {
            BaseUrl = entity.BaseUrl ?? string.Empty,
            Model = entity.Model ?? string.Empty,
            HasApiKey = hasApiKey,
            MaskedApiKey = hasApiKey
                ? MaskApiKey(apiKeyProtector.Decrypt(entity.ApiKeyEncrypted!))
                : string.Empty,
            TimeoutSeconds = entity.TimeoutSeconds,
            MaxOutputTokens = entity.MaxOutputTokens,
            Temperature = entity.Temperature
        };
    }

    private static string MaskApiKey(string apiKey)
    {
        return apiKey.Length <= 7
            ? "****"
            : $"{apiKey[..3]}****{apiKey[^4..]}";
    }

    private static AiTenantSettingDto ToTenantSettingDto(AiTenantSettingEntity entity)
    {
        return new AiTenantSettingDto
        {
            TenantId = entity.TenantId,
            Enabled = entity.Enabled,
            DailyRequestLimit = entity.DailyRequestLimit
        };
    }

    private static AiUsageDto ToUsageDto(AiUsageEntity entity)
    {
        return new AiUsageDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            UserId = entity.UserId,
            ConversationId = entity.ConversationId,
            MessageId = entity.MessageId,
            Model = entity.Model ?? string.Empty,
            InputTokens = entity.InputTokens,
            OutputTokens = entity.OutputTokens,
            TotalTokens = entity.TotalTokens,
            DurationMs = entity.DurationMs,
            Status = entity.Status,
            ErrorType = entity.ErrorType ?? string.Empty,
            CreateTime = entity.CreateTime,
            UpdateTime = entity.UpdateTime
        };
    }

    private static string GetConnectionFailureMessage(string errorType)
    {
        return errorType switch
        {
            "ai_authentication" => "身份验证失败",
            "ai_rate_limited" => "模型服务请求过于频繁",
            "ai_timeout" => "模型服务响应超时",
            "ai_cancelled" => "连接测试已取消",
            "ai_invalid_response" => "模型服务返回格式无效",
            _ => "模型服务连接失败"
        };
    }
}
