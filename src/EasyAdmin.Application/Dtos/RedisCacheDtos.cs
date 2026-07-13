using EasyAdmin.Infrastructure.Converter;
using Newtonsoft.Json;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// Redis缓存分页查询条件
/// </summary>
public class RedisCachePageReqDto : PageRequestBase
{
	/// <summary>
	/// Redis数据库索引
	/// </summary>
	public int Database { get; set; }

    /// <summary>
    /// Key匹配模式
    /// </summary>
    public string? Pattern { get; set; }
}

/// <summary>
/// Redis缓存Key信息
/// </summary>
public class RedisCacheKeyDto
{
    /// <summary>
    /// 缓存Key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 剩余有效时间
    /// </summary>
    [JsonConverter(typeof(JsonLongAsNumberConverter))]
    public long TtlSeconds { get; set; }
}

/// <summary>
/// Redis缓存详情
/// </summary>
public class RedisCacheDetailDto : RedisCacheKeyDto
{
    /// <summary>
    /// 缓存值
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Redis服务信息
/// </summary>
public class RedisServerInfoDto
{
    /// <summary>
    /// 是否连接成功
    /// </summary>
    public bool Connected { get; set; }

    /// <summary>
    /// Redis地址
    /// </summary>
    public string? EndPoints { get; set; }

    /// <summary>
    /// 当前数据库
    /// </summary>
    public int Database { get; set; }

    /// <summary>
    /// Redis数据库数量
    /// </summary>
    public int DatabaseCount { get; set; }

    /// <summary>
    /// Key数量
    /// </summary>
    [JsonConverter(typeof(JsonLongAsNumberConverter))]
    public long KeyCount { get; set; }

    /// <summary>
    /// 已使用内存
    /// </summary>
    public string? UsedMemory { get; set; }
}
