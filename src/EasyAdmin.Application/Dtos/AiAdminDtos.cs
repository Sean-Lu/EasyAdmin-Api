using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// AI模型配置
/// </summary>
public sealed class AiModelConfigDto
{
    /// <summary>
    /// 服务地址
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 是否已配置接口密钥
    /// </summary>
    public bool HasApiKey { get; set; }

    /// <summary>
    /// 脱敏接口密钥
    /// </summary>
    public string MaskedApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 超时秒数
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// 最大输出令牌
    /// </summary>
    public int MaxOutputTokens { get; set; }

    /// <summary>
    /// 生成温度
    /// </summary>
    public decimal Temperature { get; set; }
}

/// <summary>
/// AI模型配置更新参数
/// </summary>
public sealed class AiModelConfigUpdateDto
{
    /// <summary>
    /// 服务地址
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 接口密钥
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// 超时秒数
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// 最大输出令牌
    /// </summary>
    public int MaxOutputTokens { get; set; }

    /// <summary>
    /// 生成温度
    /// </summary>
    public decimal Temperature { get; set; }
}

/// <summary>
/// AI运行时配置
/// </summary>
public sealed class AiRuntimeConfig
{
    /// <summary>
    /// 服务地址
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 接口密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 超时秒数
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// 最大输出令牌
    /// </summary>
    public int MaxOutputTokens { get; set; }

    /// <summary>
    /// 生成温度
    /// </summary>
    public decimal Temperature { get; set; }

    /// <summary>
    /// 每日请求上限
    /// </summary>
    public int DailyRequestLimit { get; set; }
}

/// <summary>
/// AI连接测试结果
/// </summary>
public sealed class AiConnectionTestResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 测试结果
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// 延迟毫秒数
    /// </summary>
    public int LatencyMs { get; set; }

    /// <summary>
    /// 错误类型
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;
}

/// <summary>
/// AI租户设置
/// </summary>
public sealed class AiTenantSettingDto
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 每日请求上限
    /// </summary>
    public int DailyRequestLimit { get; set; }
}

/// <summary>
/// AI租户设置更新参数
/// </summary>
public sealed class AiTenantSettingUpdateDto
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 每日请求上限
    /// </summary>
    public int DailyRequestLimit { get; set; }
}

/// <summary>
/// AI用量记录
/// </summary>
public sealed class AiUsageDto
{
    /// <summary>
    /// 记录ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public long ConversationId { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 输入令牌数
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// 输出令牌数
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// 总令牌数
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// 耗时毫秒数
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// 请求状态
    /// </summary>
    public AiUsageStatus Status { get; set; }

    /// <summary>
    /// 错误类型
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// AI用量分页参数
/// </summary>
public sealed class AiUsagePageReqDto : PageRequestBase
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 用户关键词
    /// </summary>
    public string? UserKeyword { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 请求状态
    /// </summary>
    public AiUsageStatus? Status { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
