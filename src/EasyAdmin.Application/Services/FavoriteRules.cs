using EasyAdmin.Domain.Entities;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 收藏规则
/// </summary>
public static class FavoriteRules
{
    public sealed record FavoriteShareDisplay(string Title, string OwnerName);

    /// <summary>
    /// 映射分享目标类型
    /// </summary>
    public static FavoriteTargetType MapShareTargetType(ShareTargetType targetType)
    {
        return targetType switch
        {
            ShareTargetType.File => FavoriteTargetType.File,
            ShareTargetType.Note => FavoriteTargetType.Note,
            _ => throw new ExplicitException("不支持的分享类型")
        };
    }

    /// <summary>
    /// 校验收藏状态请求
    /// </summary>
    public static void ValidateStatusRequest(FavoriteStatusReqDto request)
    {
        var hasShareCode = !string.IsNullOrWhiteSpace(request.ShareCode);
        var hasTargets = request.Targets.Count > 0;
        if (hasShareCode == hasTargets)
        {
            throw new ExplicitException("收藏状态请求参数无效");
        }
        if (request.Targets.Count > 200)
        {
            throw new ExplicitException("单次最多查询200个收藏状态");
        }
        if (request.Targets.Any(item => item.TargetId < 1))
        {
            throw new ExplicitException("收藏目标无效");
        }
    }

    /// <summary>
    /// 获取分享收藏显示信息
    /// </summary>
    public static FavoriteShareDisplay ResolveShareDisplay(
        FavoriteAvailabilityStatus status,
        string? liveTitle,
        string? liveOwnerName,
        string? titleSnapshot,
        string? ownerNameSnapshot)
    {
        return status == FavoriteAvailabilityStatus.Normal
            ? new FavoriteShareDisplay(liveTitle ?? string.Empty, liveOwnerName ?? string.Empty)
            : new FavoriteShareDisplay(titleSnapshot ?? string.Empty, ownerNameSnapshot ?? string.Empty);
    }

    /// <summary>
    /// 判断菜单是否可收藏
    /// </summary>
    public static bool IsCollectibleMenu(MenuEntity? menu)
    {
        return menu is { Id: > 0, IsDelete: false, State: CommonState.Enable }
               && menu.Type != MenuType.Directory
               && !string.IsNullOrWhiteSpace(menu.Path)
               && (menu.Type != MenuType.External || !string.IsNullOrWhiteSpace(menu.OutLink));
    }

    /// <summary>
    /// 判断工具是否可收藏
    /// </summary>
    public static bool IsCollectibleTool(long toolId, IEnumerable<MenuEntity> menus)
    {
        return ToolboxToolCatalog.Find(toolId) != null &&
               menus.Any(menu => menu.Path == ToolboxToolCatalog.ToolboxPath && IsCollectibleMenu(menu));
    }

    /// <summary>
    /// 获取分享收藏状态
    /// </summary>
    public static FavoriteAvailabilityStatus GetShareStatus(ShareEntity share, bool targetAvailable, DateTime now)
    {
        if (!targetAvailable)
        {
            return FavoriteAvailabilityStatus.ShareTargetDeleted;
        }
        if (!share.IsEnabled || share.IsDelete)
        {
            return FavoriteAvailabilityStatus.ShareDisabled;
        }
        return share.ExpiresAt.HasValue && share.ExpiresAt.Value <= now
            ? FavoriteAvailabilityStatus.ShareExpired
            : FavoriteAvailabilityStatus.Normal;
    }
}
