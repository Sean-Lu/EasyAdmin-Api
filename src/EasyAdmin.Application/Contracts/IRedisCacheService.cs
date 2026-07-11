using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// Redis缓存管理服务
/// </summary>
public interface IRedisCacheService
{
    Task<RedisServerInfoDto> GetServerInfoAsync();
    Task<ApiResultPageData<RedisCacheKeyDto>> PageKeysAsync(RedisCachePageReqDto request);
    Task<RedisCacheDetailDto?> GetDetailAsync(string key);
    Task<bool> DeleteAsync(string key);
    Task<long> DeleteByPatternAsync(string pattern);
    Task ClearDatabaseAsync();
}
