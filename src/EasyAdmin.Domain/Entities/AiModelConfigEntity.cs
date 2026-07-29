using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// AI模型配置表
/// </summary>
[CodeFirst]
[Index(new[] { nameof(ConfigKey) }, "UX_AiModelConfig_ConfigKey", DbIndexType.Unique)]
public class AiModelConfigEntity : EntityBase
{
    /// <summary>
    /// 配置键
    /// </summary>
    [Required]
    [MaxLength(50)]
    [DefaultValue("default")]
    public string ConfigKey { get; private set; } = "default";

    /// <summary>
    /// 模型
    /// </summary>
    [MaxLength(100)]
    public string? Model { get; set; }

    /// <summary>
    /// 服务地址
    /// </summary>
    [MaxLength(500)]
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 密钥
    /// </summary>
    [MaxLength(1000)]
    public string? ApiKeyEncrypted { get; set; }

    /// <summary>
    /// 超时时间
    /// </summary>
    [DefaultValue(60)]
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// 最大输出令牌数
    /// </summary>
    [DefaultValue(4096)]
    public int MaxOutputTokens { get; set; } = 4096;

    /// <summary>
    /// 温度
    /// </summary>
    [DefaultValue(typeof(decimal), "0.7")]
    public decimal Temperature { get; set; } = 0.7m;
}