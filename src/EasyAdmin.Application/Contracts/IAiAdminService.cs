using EasyAdmin.Application.Dtos;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// AI管理服务接口
/// </summary>
public interface IAiAdminService
{
    /// <summary>
    /// 获取模型配置
    /// </summary>
    Task<AiModelConfigDto> GetModelConfigAsync();

    /// <summary>
    /// 更新模型配置
    /// </summary>
    Task<AiModelConfigDto> UpdateModelConfigAsync(AiModelConfigUpdateDto request);

    /// <summary>
    /// 获取租户AI设置
    /// </summary>
    Task<AiTenantSettingDto> GetTenantSettingAsync(long tenantId);

    /// <summary>
    /// 更新租户AI设置
    /// </summary>
    Task<AiTenantSettingDto> UpdateTenantSettingAsync(AiTenantSettingUpdateDto request);

    /// <summary>
    /// 分页查询AI用量
    /// </summary>
    Task<PageQueryResult<AiUsageDto>> PageUsageAsync(AiUsagePageReqDto request);

    /// <summary>
    /// 获取租户运行时配置
    /// </summary>
    Task<AiRuntimeConfig> GetRuntimeConfigAsync(long tenantId);

    /// <summary>
    /// 测试模型连接
    /// </summary>
    Task<AiConnectionTestResultDto> TestConnectionAsync();
}