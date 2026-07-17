using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 收藏服务
/// </summary>
public class FavoriteService(
    IFavoriteRepository favoriteRepository,
    IMenuService menuService,
    IFileRepository fileRepository,
    INoteRepository noteRepository,
    IShareRepository shareRepository,
    IUserRepository userRepository,
    IShareService shareService) : IFavoriteService
{
    public async Task<PageQueryResult<FavoriteListItemDto>> PageAsync(FavoritePageReqDto request)
    {
        if (!Enum.IsDefined(request.TargetType))
        {
            throw new ExplicitException("收藏类型无效");
        }
        if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
        {
            throw new ExplicitException("收藏状态无效");
        }

        var tenantId = TenantContextHolder.TenantId;
        var userId = TenantContextHolder.UserId;
        var favorites = (await favoriteRepository.QueryAsync(entity =>
            entity.TenantId == tenantId &&
            entity.CreateUserId == userId &&
            entity.TargetType == request.TargetType &&
            !entity.IsDelete))?.ToList() ?? new List<FavoriteEntity>();

        var menuTree = await menuService.GetMenuTreeAsync(userId, new MenuListReqDto());
        var accessibleMenus = FlattenMenus(menuTree)
            .Where(FavoriteRules.IsCollectibleMenu)
            .ToDictionary(entity => entity.Id);
        var directIds = favorites.Where(entity => entity.SourceType == FavoriteSourceType.Direct)
            .Select(entity => entity.TargetId).Distinct().ToList();
        var directFiles = request.TargetType == FavoriteTargetType.File && directIds.Count > 0
            ? (await fileRepository.QueryAsync(entity => directIds.Contains(entity.Id) && entity.TenantId == tenantId && !entity.IsDelete))?.ToList()
              ?? new List<FileEntity>()
            : new List<FileEntity>();
        var directNotes = request.TargetType == FavoriteTargetType.Note && directIds.Count > 0
            ? (await noteRepository.QueryAsync(entity => directIds.Contains(entity.Id) && entity.TenantId == tenantId && entity.UserId == userId && !entity.IsDelete))?.ToList()
              ?? new List<NoteEntity>()
            : new List<NoteEntity>();
        var directFileMap = directFiles.ToDictionary(entity => entity.Id);
        var directNoteMap = directNotes.ToDictionary(entity => entity.Id);

        var shareIds = favorites.Where(entity => entity.SourceType == FavoriteSourceType.Share)
            .Select(entity => entity.TargetId).Distinct().ToList();
        var shares = shareIds.Count == 0
            ? new List<ShareEntity>()
            : (await shareRepository.QueryAsync(entity => shareIds.Contains(entity.Id)))?.ToList() ?? new List<ShareEntity>();
        var shareMap = shares.ToDictionary(entity => entity.Id);
        var sharedFileIds = shares.Where(entity => entity.TargetType == ShareTargetType.File)
            .Select(entity => entity.TargetId).Distinct().ToList();
        var sharedNoteIds = shares.Where(entity => entity.TargetType == ShareTargetType.Note)
            .Select(entity => entity.TargetId).Distinct().ToList();
        var sharedFiles = sharedFileIds.Count == 0
            ? new List<FileEntity>()
            : (await fileRepository.QueryAsync(entity => sharedFileIds.Contains(entity.Id)))?.ToList() ?? new List<FileEntity>();
        var sharedNotes = sharedNoteIds.Count == 0
            ? new List<NoteEntity>()
            : (await noteRepository.QueryAsync(entity => sharedNoteIds.Contains(entity.Id)))?.ToList() ?? new List<NoteEntity>();
        var sharedFileMap = sharedFiles.ToDictionary(entity => entity.Id);
        var sharedNoteMap = sharedNotes.ToDictionary(entity => entity.Id);
        var ownerIds = shares.Select(entity => entity.CreateUserId).Distinct().ToList();
        var owners = ownerIds.Count == 0
            ? new List<UserEntity>()
            : (await userRepository.QueryAsync(entity => ownerIds.Contains(entity.Id) && !entity.IsDelete))?.ToList() ?? new List<UserEntity>();

        var invalidDirectIds = new List<long>();
        var items = new List<FavoriteListItemDto>();
        foreach (var favorite in favorites)
        {
            if (favorite.SourceType == FavoriteSourceType.Direct)
            {
                var directItem = BuildDirectItem(
                    favorite,
                    accessibleMenus,
                    directFileMap,
                    directNoteMap,
                    tenantId,
                    userId);
                if (directItem == null)
                {
                    invalidDirectIds.Add(favorite.Id);
                }
                else
                {
                    items.Add(directItem);
                }
                continue;
            }

            items.Add(BuildShareItem(
                favorite,
                shareMap,
                sharedFileMap,
                sharedNoteMap,
                owners,
                DateTime.UtcNow));
        }

        if (invalidDirectIds.Count > 0)
        {
            await favoriteRepository.UpdateAsync(
                new FavoriteEntity { IsDelete = true },
                entity => entity.IsDelete,
                entity => invalidDirectIds.Contains(entity.Id) &&
                          entity.TenantId == tenantId &&
                          entity.CreateUserId == userId);
        }

        var keyword = request.Keyword?.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            items = items.Where(item => item.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (request.Status.HasValue)
        {
            items = items.Where(item => item.Status == request.Status.Value).ToList();
        }
        items = items.OrderByDescending(item => item.CreateTime).ThenByDescending(item => item.Id).ToList();
        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        return new PageQueryResult<FavoriteListItemDto>
        {
            Total = items.Count,
            List = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
    }

    public async Task<FavoriteMutationResultDto> AddAsync(FavoriteTargetReqDto request)
    {
        if (request.TargetId < 1 || !Enum.IsDefined(request.TargetType))
        {
            throw new ExplicitException("收藏目标无效");
        }
        await ValidateDirectTargetAsync(request);
        return await AddOrRestoreAsync(request.TargetType, FavoriteSourceType.Direct, request.TargetId, null, null);
    }

    public async Task<FavoriteMutationResultDto> AddShareAsync(FavoriteAddShareReqDto request)
    {
        var target = await shareService.GetFavoriteTargetAsync(request.ShareCode, request.AccessToken);
        return await AddOrRestoreAsync(
            target.TargetType,
            FavoriteSourceType.Share,
            target.ShareId,
            target.Title,
            target.OwnerName);
    }

    public async Task<FavoriteMutationResultDto> DeleteAsync(FavoriteDeleteReqDto request)
    {
        var favorite = await favoriteRepository.GetAsync(entity =>
            entity.Id == request.Id &&
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.CreateUserId == TenantContextHolder.UserId);
        if (favorite == null || favorite.Id < 1 || favorite.IsDelete)
        {
            return new FavoriteMutationResultDto { IsFavorite = false };
        }

        await favoriteRepository.UpdateAsync(
            new FavoriteEntity { IsDelete = true },
            entity => entity.IsDelete,
            entity => entity.Id == favorite.Id &&
                      entity.TenantId == TenantContextHolder.TenantId &&
                      entity.CreateUserId == TenantContextHolder.UserId);
        return new FavoriteMutationResultDto { IsFavorite = false };
    }

    public async Task<List<FavoriteStatusItemDto>> GetStatusAsync(FavoriteStatusReqDto request)
    {
        FavoriteRules.ValidateStatusRequest(request);
        if (!string.IsNullOrWhiteSpace(request.ShareCode))
        {
            return await GetShareStatusAsync(request.ShareCode.Trim());
        }

        var targets = request.Targets
            .DistinctBy(item => new { item.TargetType, item.TargetId })
            .ToList();
        var targetIds = targets.Select(item => item.TargetId).Distinct().ToList();
        var favorites = (await favoriteRepository.QueryAsync(entity =>
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.CreateUserId == TenantContextHolder.UserId &&
            entity.SourceType == FavoriteSourceType.Direct &&
            targetIds.Contains(entity.TargetId) &&
            !entity.IsDelete))?.ToList() ?? new List<FavoriteEntity>();
        var favoriteMap = favorites.ToDictionary(entity => (entity.TargetType, entity.TargetId));

        return targets.Select(target =>
        {
            favoriteMap.TryGetValue((target.TargetType, target.TargetId), out var favorite);
            return new FavoriteStatusItemDto
            {
                TargetType = target.TargetType,
                SourceType = FavoriteSourceType.Direct,
                TargetId = target.TargetId,
                IsFavorite = favorite != null,
                FavoriteId = favorite?.Id
            };
        }).ToList();
    }

    private async Task<List<FavoriteStatusItemDto>> GetShareStatusAsync(string shareCode)
    {
        var share = await shareRepository.GetAsync(entity => entity.ShareCode == shareCode);
        if (share == null || share.Id < 1)
        {
            return new List<FavoriteStatusItemDto>();
        }
        var targetType = FavoriteRules.MapShareTargetType(share.TargetType);
        var favorite = await GetOwnedAsync(targetType, FavoriteSourceType.Share, share.Id);
        return new List<FavoriteStatusItemDto>
        {
            new()
            {
                TargetType = targetType,
                SourceType = FavoriteSourceType.Share,
                TargetId = share.Id,
                ShareCode = shareCode,
                IsFavorite = favorite is { Id: > 0, IsDelete: false },
                FavoriteId = favorite is { Id: > 0, IsDelete: false } ? favorite.Id : null
            }
        };
    }

    private async Task ValidateDirectTargetAsync(FavoriteTargetReqDto request)
    {
        var menuTree = await menuService.GetMenuTreeAsync(
            TenantContextHolder.UserId,
            new MenuListReqDto());
        var menus = FlattenMenus(menuTree).ToList();
        var allowed = request.TargetType switch
        {
            FavoriteTargetType.Menu => FavoriteRules.IsCollectibleMenu(
                menus.FirstOrDefault(menu => menu.Id == request.TargetId)),
            FavoriteTargetType.File => menus.Any(menu => menu.Path == "/system/file") &&
                                       IsCollectibleFile(await fileRepository.GetByIdAsync(request.TargetId)),
            FavoriteTargetType.Note => IsCollectibleNote(await noteRepository.GetByIdAsync(request.TargetId)),
            _ => false
        };
        if (!allowed)
        {
            throw new ExplicitException("收藏目标不存在或无权访问");
        }
    }

    private static FavoriteListItemDto? BuildDirectItem(
        FavoriteEntity favorite,
        IReadOnlyDictionary<long, MenuEntity> menuMap,
        IReadOnlyDictionary<long, FileEntity> fileMap,
        IReadOnlyDictionary<long, NoteEntity> noteMap,
        long tenantId,
        long userId)
    {
        if (favorite.TargetType == FavoriteTargetType.Menu && menuMap.TryGetValue(favorite.TargetId, out var menu))
        {
            return CreateBaseItem(favorite, menu.Title, menu.Icon, null, menu.Path, menu.OutLink, menu.OutLinkOpenType);
        }
        if (favorite.TargetType == FavoriteTargetType.File &&
            fileMap.TryGetValue(favorite.TargetId, out var file) &&
            file.TenantId == tenantId && file.BizType == FileBizType.Normal && !file.IsDelete)
        {
            return CreateBaseItem(favorite, file.Name, null, file.ContentType, null, null, null);
        }
        if (favorite.TargetType == FavoriteTargetType.Note &&
            noteMap.TryGetValue(favorite.TargetId, out var note) &&
            note.TenantId == tenantId && note.UserId == userId && !note.IsDelete)
        {
            return CreateBaseItem(favorite, note.Title, null, null, null, null, null);
        }
        return null;
    }

    private static FavoriteListItemDto BuildShareItem(
        FavoriteEntity favorite,
        IReadOnlyDictionary<long, ShareEntity> shareMap,
        IReadOnlyDictionary<long, FileEntity> fileMap,
        IReadOnlyDictionary<long, NoteEntity> noteMap,
        IReadOnlyList<UserEntity> owners,
        DateTime now)
    {
        shareMap.TryGetValue(favorite.TargetId, out var share);
        var targetAvailable = false;
        string? liveTitle = null;
        string? contentType = null;
        string? liveOwnerName = null;
        if (share != null && FavoriteRules.MapShareTargetType(share.TargetType) == favorite.TargetType)
        {
            if (share.TargetType == ShareTargetType.File && fileMap.TryGetValue(share.TargetId, out var file))
            {
                targetAvailable = ShareTargetPolicy.CanShareFile(file, share.TenantId, share.CreateUserId);
                liveTitle = targetAvailable ? file.Name : null;
                contentType = targetAvailable ? file.ContentType : null;
            }
            else if (share.TargetType == ShareTargetType.Note && noteMap.TryGetValue(share.TargetId, out var note))
            {
                targetAvailable = ShareTargetPolicy.CanShareNote(note, share.TenantId, share.CreateUserId);
                liveTitle = targetAvailable ? note.Title : null;
            }
            var owner = owners.FirstOrDefault(entity => entity.Id == share.CreateUserId && entity.TenantId == share.TenantId);
            liveOwnerName = ShareOwnerDisplayName.Resolve(owner);
        }

        var status = share == null
            ? FavoriteAvailabilityStatus.ShareTargetDeleted
            : FavoriteRules.GetShareStatus(share, targetAvailable, now);
        var display = FavoriteRules.ResolveShareDisplay(
            status,
            liveTitle,
            liveOwnerName,
            favorite.TitleSnapshot,
            favorite.OwnerNameSnapshot);
        return new FavoriteListItemDto
        {
            Id = favorite.Id,
            TargetType = favorite.TargetType,
            SourceType = FavoriteSourceType.Share,
            Title = display.Title,
            OwnerName = display.OwnerName,
            ContentType = status == FavoriteAvailabilityStatus.Normal ? contentType : null,
            ShareCode = status == FavoriteAvailabilityStatus.Normal ? share?.ShareCode : null,
            Status = status,
            IsAvailable = status == FavoriteAvailabilityStatus.Normal,
            CreateTime = favorite.CreateTime
        };
    }

    private static FavoriteListItemDto CreateBaseItem(
        FavoriteEntity favorite,
        string title,
        string? icon,
        string? contentType,
        string? path,
        string? outLink,
        OutLinkOpenType? outLinkOpenType)
    {
        return new FavoriteListItemDto
        {
            Id = favorite.Id,
            TargetType = favorite.TargetType,
            SourceType = FavoriteSourceType.Direct,
            DirectTargetId = favorite.TargetId,
            Title = title,
            Icon = icon,
            ContentType = contentType,
            Path = path,
            OutLink = outLink,
            OutLinkOpenType = outLinkOpenType,
            Status = FavoriteAvailabilityStatus.Normal,
            IsAvailable = true,
            CreateTime = favorite.CreateTime
        };
    }

    private static IEnumerable<MenuEntity> FlattenMenus(IEnumerable<MenuEntity> menus)
    {
        foreach (var menu in menus)
        {
            yield return menu;
            if (menu.Children == null)
            {
                continue;
            }
            foreach (var child in FlattenMenus(menu.Children))
            {
                yield return child;
            }
        }
    }

    private static bool IsCollectibleFile(FileEntity? file)
    {
        return file is { Id: > 0, IsDelete: false, BizType: FileBizType.Normal }
               && file.TenantId == TenantContextHolder.TenantId;
    }

    private static bool IsCollectibleNote(NoteEntity? note)
    {
        return note is { Id: > 0, IsDelete: false }
               && note.TenantId == TenantContextHolder.TenantId
               && note.UserId == TenantContextHolder.UserId;
    }

    private async Task<FavoriteMutationResultDto> AddOrRestoreAsync(
        FavoriteTargetType targetType,
        FavoriteSourceType sourceType,
        long targetId,
        string? titleSnapshot,
        string? ownerNameSnapshot)
    {
        var existing = await GetOwnedAsync(targetType, sourceType, targetId);
        if (existing is { Id: > 0 })
        {
            if (existing.IsDelete || existing.TitleSnapshot != titleSnapshot || existing.OwnerNameSnapshot != ownerNameSnapshot)
            {
                await favoriteRepository.UpdateAsync(
                    new FavoriteEntity
                    {
                        IsDelete = false,
                        TitleSnapshot = titleSnapshot,
                        OwnerNameSnapshot = ownerNameSnapshot
                    },
                    entity => new { entity.IsDelete, entity.TitleSnapshot, entity.OwnerNameSnapshot },
                    entity => entity.Id == existing.Id &&
                              entity.TenantId == TenantContextHolder.TenantId &&
                              entity.CreateUserId == TenantContextHolder.UserId);
            }
            return new FavoriteMutationResultDto { IsFavorite = true, FavoriteId = existing.Id };
        }

        var favorite = new FavoriteEntity
        {
            TenantId = TenantContextHolder.TenantId,
            CreateUserId = TenantContextHolder.UserId,
            TargetType = targetType,
            SourceType = sourceType,
            TargetId = targetId,
            TitleSnapshot = titleSnapshot,
            OwnerNameSnapshot = ownerNameSnapshot
        };
        try
        {
            if (await favoriteRepository.AddAsync(favorite))
            {
                return new FavoriteMutationResultDto { IsFavorite = true, FavoriteId = favorite.Id };
            }
        }
        catch
        {
            existing = await GetOwnedAsync(targetType, sourceType, targetId);
            if (existing is not { Id: > 0, IsDelete: false })
            {
                throw;
            }
            return new FavoriteMutationResultDto { IsFavorite = true, FavoriteId = existing.Id };
        }
        throw new ExplicitException("收藏失败");
    }

    private Task<FavoriteEntity?> GetOwnedAsync(
        FavoriteTargetType targetType,
        FavoriteSourceType sourceType,
        long targetId)
    {
        return favoriteRepository.GetAsync(entity =>
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.CreateUserId == TenantContextHolder.UserId &&
            entity.TargetType == targetType &&
            entity.SourceType == sourceType &&
            entity.TargetId == targetId);
    }
}
