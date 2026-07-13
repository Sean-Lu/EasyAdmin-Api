using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// Redis缓存管理服务
/// </summary>
public interface IRedisCacheService
{
    Task<RedisServerInfoDto> GetServerInfoAsync(int database = 0);
    Task<ApiResultPageData<RedisCacheKeyDto>> PageKeysAsync(RedisCachePageReqDto request);
    Task<RedisCacheDetailDto?> GetDetailAsync(string key, int database = 0);
    Task<bool> DeleteAsync(string key, int database = 0);
    Task<long> DeleteByPatternAsync(string pattern, int database = 0);
    Task ClearDatabaseAsync(int database = 0);
}
