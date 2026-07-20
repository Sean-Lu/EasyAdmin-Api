using System.Text.Json;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 用户偏好服务
/// </summary>
public class UserPreferenceService(IUserPreferenceRepository userPreferenceRepository) : IUserPreferenceService
{
    private const string ToolboxToolOrderKey = "toolbox.tool-order";

    public async Task<ToolboxToolOrderDto> GetToolboxToolOrderAsync()
    {
        var preference = await GetOwnedAsync();
        List<long>? storedOrder = null;
        if (preference is { Id: > 0 } && !string.IsNullOrWhiteSpace(preference.PreferenceValue))
        {
            try
            {
                storedOrder = JsonSerializer.Deserialize<List<long>>(preference.PreferenceValue);
            }
            catch (JsonException)
            {
                storedOrder = null;
            }
        }
        return new ToolboxToolOrderDto
        {
            ToolIds = NormalizeStoredToolOrder(storedOrder)
        };
    }

    public async Task<ToolboxToolOrderDto> UpdateToolboxToolOrderAsync(ToolboxToolOrderDto request)
    {
        var toolIds = NormalizeSubmittedToolOrder(request.ToolIds);
        var preferenceValue = JsonSerializer.Serialize(toolIds);
        var existing = await GetOwnedAsync();
        if (existing is { Id: > 0 })
        {
            await UpdateOwnedAsync(existing.Id, preferenceValue);
            return new ToolboxToolOrderDto { ToolIds = toolIds };
        }

        var preference = new UserPreferenceEntity
        {
            TenantId = TenantContextHolder.TenantId,
            CreateUserId = TenantContextHolder.UserId,
            PreferenceKey = ToolboxToolOrderKey,
            PreferenceValue = preferenceValue
        };
        try
        {
            if (await userPreferenceRepository.AddAsync(preference))
            {
                return new ToolboxToolOrderDto { ToolIds = toolIds };
            }
        }
        catch
        {
            existing = await GetOwnedAsync();
            if (existing is not { Id: > 0 })
            {
                throw;
            }
            await UpdateOwnedAsync(existing.Id, preferenceValue);
            return new ToolboxToolOrderDto { ToolIds = toolIds };
        }
        throw new ExplicitException("保存用户偏好失败");
    }

    private Task<UserPreferenceEntity?> GetOwnedAsync()
    {
        return userPreferenceRepository.GetAsync(entity =>
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.CreateUserId == TenantContextHolder.UserId &&
            entity.PreferenceKey == ToolboxToolOrderKey);
    }

    private Task<int> UpdateOwnedAsync(long id, string preferenceValue)
    {
        return userPreferenceRepository.UpdateAsync(
            new UserPreferenceEntity
            {
                PreferenceValue = preferenceValue,
                UpdateUserId = TenantContextHolder.UserId,
                UpdateTime = DateTime.Now
            },
            entity => new { entity.PreferenceValue, entity.UpdateUserId, entity.UpdateTime },
            entity => entity.Id == id &&
                      entity.TenantId == TenantContextHolder.TenantId &&
                      entity.CreateUserId == TenantContextHolder.UserId &&
                      entity.PreferenceKey == ToolboxToolOrderKey);
    }

    private static List<long> NormalizeStoredToolOrder(IEnumerable<long>? toolIds)
    {
        var catalogIds = ToolboxToolCatalog.All.Select(tool => tool.Id).ToList();
        var catalogIdSet = catalogIds.ToHashSet();
        var result = (toolIds ?? Array.Empty<long>())
            .Where(catalogIdSet.Contains)
            .Distinct()
            .ToList();
        result.AddRange(catalogIds.Where(id => !result.Contains(id)));
        return result;
    }

    private static List<long> NormalizeSubmittedToolOrder(IEnumerable<long>? toolIds)
    {
        var submitted = (toolIds ?? Array.Empty<long>()).ToList();
        var catalogIds = ToolboxToolCatalog.All.Select(tool => tool.Id).ToHashSet();
        if (submitted.Count != submitted.Distinct().Count() || submitted.Any(id => !catalogIds.Contains(id)))
        {
            throw new ExplicitException("工具排序数据无效");
        }
        return NormalizeStoredToolOrder(submitted);
    }
}
