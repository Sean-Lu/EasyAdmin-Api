using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Storage;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 分享服务
/// </summary>
public class ShareService(
    IShareRepository shareRepository,
    IFileRepository fileRepository,
    IFileService fileService,
    INoteRepository noteRepository,
    IUserRepository userRepository,
    INoteService noteService,
    IFileStorageFactory fileStorageFactory,
    SharePasswordProtector passwordProtector) : IShareService
{
    /// <summary>
    /// 获取我的分享
    /// </summary>
    public async Task<PageQueryResult<ShareListItemDto>> ListAsync(ShareListReqDto request)
    {
        var tenantId = TenantContextHolder.TenantId;
        var userId = TenantContextHolder.UserId;
        var shares = (await shareRepository.QueryAsync(entity =>
            entity.TenantId == tenantId &&
            entity.CreateUserId == userId &&
            !entity.IsDelete))?.ToList() ?? new List<ShareEntity>();

        var fileIds = shares.Where(entity => entity.TargetType == ShareTargetType.File)
            .Select(entity => entity.TargetId).Distinct().ToList();
        var noteIds = shares.Where(entity => entity.TargetType == ShareTargetType.Note)
            .Select(entity => entity.TargetId).Distinct().ToList();
        var files = fileIds.Count == 0
            ? new List<FileEntity>()
            : (await fileRepository.QueryAsync(entity => fileIds.Contains(entity.Id) && entity.TenantId == tenantId))?.ToList() ?? new List<FileEntity>();
        var notes = noteIds.Count == 0
            ? new List<NoteEntity>()
            : (await noteRepository.QueryAsync(entity => noteIds.Contains(entity.Id) && entity.TenantId == tenantId))?.ToList() ?? new List<NoteEntity>();
        var fileMap = files.ToDictionary(entity => entity.Id);
        var noteMap = notes.ToDictionary(entity => entity.Id);
        var now = DateTime.UtcNow;

        var items = shares.Select(share =>
        {
            var targetAvailable = share.TargetType switch
            {
                ShareTargetType.File => fileMap.TryGetValue(share.TargetId, out var file) &&
                                        ShareTargetPolicy.CanShareFile(file, tenantId, userId),
                ShareTargetType.Note => noteMap.TryGetValue(share.TargetId, out var note) &&
                                        ShareTargetPolicy.CanShareNote(note, tenantId, userId),
                _ => false
            };
            var targetName = share.TargetType switch
            {
                ShareTargetType.File when fileMap.TryGetValue(share.TargetId, out var file) => file.Name,
                ShareTargetType.Note when noteMap.TryGetValue(share.TargetId, out var note) => note.Title,
                _ => string.Empty
            };
            return new ShareListItemDto
            {
                Id = share.Id,
                TargetType = share.TargetType,
                TargetId = share.TargetId,
                TargetName = targetAvailable ? targetName : "内容已删除",
                ShareCode = share.ShareCode,
                IsEnabled = share.IsEnabled,
                HasPassword = HasPassword(share),
                CreateTime = share.CreateTime,
                ExpiresAt = share.ExpiresAt,
                Status = ShareLifecycle.GetListStatus(share.IsEnabled, share.ExpiresAt, targetAvailable, now),
                TargetAvailable = targetAvailable
            };
        }).ToList();

        var keyword = request.Keyword?.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            items = items.Where(item => item.TargetName.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (request.TargetType.HasValue)
        {
            items = items.Where(item => item.TargetType == request.TargetType.Value).ToList();
        }
        if (request.Status.HasValue)
        {
            items = items.Where(item => item.Status == request.Status.Value).ToList();
        }

        items = items.OrderByDescending(item => item.CreateTime).ThenByDescending(item => item.Id).ToList();
        var pageNumber = Math.Max(request.PageNumber, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        return new PageQueryResult<ShareListItemDto>
        {
            Total = items.Count(),
            List = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
    }

    public async Task<ShareConfigDto> GetConfigAsync(ShareTargetType targetType, long targetId)
    {
        await ValidateTargetAsync(targetType, targetId, TenantContextHolder.TenantId, TenantContextHolder.UserId);
        return ToConfig(await GetOwnedShareAsync(targetType, targetId));
    }

    public async Task<ShareConfigDto> SaveAsync(ShareSaveDto request)
    {
        var now = DateTime.UtcNow;
        ShareLifecycle.ValidateExpiry(request.ExpiresAt, now);
        await ValidateTargetAsync(request.TargetType, request.TargetId, TenantContextHolder.TenantId, TenantContextHolder.UserId);

        var share = await GetOwnedShareAsync(request.TargetType, request.TargetId);
        if (share == null || share.Id < 1)
        {
            share = new ShareEntity
            {
                TenantId = TenantContextHolder.TenantId,
                CreateUserId = TenantContextHolder.UserId,
                ShareCode = ShareSecurity.CreateCode(),
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                ExpiresAt = request.ExpiresAt,
                IsEnabled = request.IsEnabled,
                AccessVersion = 1
            };
            ApplyPassword(share, request.Password);
            if (!await shareRepository.AddAsync(share))
            {
                throw new ExplicitException("保存分享失败");
            }
            return ToConfig(share);
        }

        share.IsDelete = false;
        share.ExpiresAt = request.ExpiresAt;
        ShareLifecycle.ApplyEnabled(share, request.IsEnabled);
        ApplyPassword(share, request.Password);
        await PersistAsync(share);
        return ToConfig(share);
    }

    public async Task<ShareConfigDto> SetEnabledAsync(ShareToggleDto request)
    {
        await ValidateTargetAsync(request.TargetType, request.TargetId, TenantContextHolder.TenantId, TenantContextHolder.UserId);
        var share = await GetRequiredOwnedShareAsync(request.TargetType, request.TargetId);
        ShareLifecycle.ApplyEnabled(share, request.IsEnabled);
        await PersistAsync(share);
        return ToConfig(share);
    }

    public async Task<ShareConfigDto> RegenerateAsync(ShareTargetRequestDto request)
    {
        await ValidateTargetAsync(request.TargetType, request.TargetId, TenantContextHolder.TenantId, TenantContextHolder.UserId);
        var share = await GetRequiredOwnedShareAsync(request.TargetType, request.TargetId);
        ShareLifecycle.RegenerateCode(share);
        await PersistAsync(share);
        return ToConfig(share);
    }

    public async Task<PublicShareStatusDto> GetPublicStatusAsync(string shareCode)
    {
        var share = await GetAvailableShareAsync(shareCode);
        return new PublicShareStatusDto
        {
            TargetType = share.TargetType,
            RequiresPassword = HasPassword(share)
        };
    }

    public async Task<PublicShareVerifyResultDto> VerifyPasswordAsync(PublicShareVerifyDto request, string ipAddress)
    {
        var share = await GetAvailableShareAsync(request.ShareCode);
        if (!HasPassword(share))
        {
            throw new ExplicitException("该分享无需密码");
        }

        await ShareSecurity.EnsurePasswordAttemptAllowedAsync(share.ShareCode, ipAddress);
        if (!passwordProtector.IsMatch(request.Password, share.PasswordCiphertext!))
        {
            await ShareSecurity.RegisterPasswordFailureAsync(share.ShareCode, ipAddress);
        }

        await ShareSecurity.ClearPasswordFailuresAsync(share.ShareCode, ipAddress);
        var (accessToken, expireMinutes) = await ShareSecurity.CreateAccessTokenAsync(
            share.ShareCode, share.AccessVersion, share.ExpiresAt, DateTime.UtcNow);
        return new PublicShareVerifyResultDto { AccessToken = accessToken, ExpireMinutes = expireMinutes };
    }

    public async Task<PublicShareFileDto> GetPublicFileAsync(string shareCode, string? accessToken)
    {
        var (share, file) = await GetPublicFileTargetAsync(shareCode, accessToken);
        return new PublicShareFileDto
        {
            OwnerName = await GetOwnerNameAsync(share),
            Name = file.Name,
            Size = file.Size,
            ContentType = file.ContentType,
            ExpiresAt = share.ExpiresAt
        };
    }

    public async Task<PublicShareNoteDto> GetPublicNoteAsync(string shareCode, string? accessToken)
    {
        var share = await GetPublicShareAsync(shareCode, accessToken, ShareTargetType.Note);
        var note = await noteService.GetSharedDetailAsync(share.TargetId, share.TenantId, share.CreateUserId);
        if (note == null)
        {
            throw ShareUnavailable();
        }

        return new PublicShareNoteDto
        {
            OwnerName = await GetOwnerNameAsync(share),
            Title = note.Title,
            ContentType = note.ContentType,
            ContentHtml = note.ContentHtml,
            CategoryName = note.CategoryName,
            Tags = note.Tags.Select(tag => tag.Name).ToList(),
            UpdateTime = note.UpdateTime,
            ExpiresAt = share.ExpiresAt
        };
    }

    public async Task<PublicShareStream> OpenPublicFileAsync(string shareCode, string? accessToken)
    {
        var (_, file) = await GetPublicFileTargetAsync(shareCode, accessToken);
        return await OpenFileAsync(file);
    }

    public async Task<PublicShareStream> OpenPublicNoteImageAsync(string shareCode, long fileId, string? accessToken)
    {
        var share = await GetPublicShareAsync(shareCode, accessToken, ShareTargetType.Note);
        var note = await noteRepository.GetByIdAsync(share.TargetId);
        var image = await fileService.GetByIdAsync(fileId);
        var imageIds = await noteService.GetSharedImageIdsAsync(share.TargetId, share.TenantId, share.CreateUserId);
        if (!SharePublicAccess.CanAccessNoteImage(note, image, imageIds) ||
            note.TenantId != share.TenantId || note.UserId != share.CreateUserId)
        {
            throw ShareUnavailable();
        }
        return await OpenFileAsync(image);
    }

    private async Task ValidateTargetAsync(ShareTargetType targetType, long targetId, long tenantId, long userId)
    {
        var canShare = targetType switch
        {
            ShareTargetType.File => ShareTargetPolicy.CanShareFile(await fileService.GetByIdAsync(targetId), tenantId, userId),
            ShareTargetType.Note => ShareTargetPolicy.CanShareNote(await noteRepository.GetByIdAsync(targetId), tenantId, userId),
            _ => false
        };
        if (!canShare)
        {
            throw new ExplicitException("只能分享本人拥有的文件或笔记");
        }
    }

    private async Task<ShareEntity> GetAvailableShareAsync(string shareCode)
    {
        if (string.IsNullOrWhiteSpace(shareCode))
        {
            throw ShareUnavailable();
        }

        var share = await shareRepository.GetAsync(entity => entity.ShareCode == shareCode);
        if (SharePublicAccess.IsUnavailable(share, DateTime.UtcNow) || !await IsTargetAvailableAsync(share!))
        {
            throw ShareUnavailable();
        }
        return share!;
    }

    private async Task<ShareEntity> GetPublicShareAsync(string shareCode, string? accessToken, ShareTargetType targetType)
    {
        var share = await GetAvailableShareAsync(shareCode);
        if (share.TargetType != targetType)
        {
            throw ShareUnavailable();
        }
        if (HasPassword(share) && !await ShareSecurity.ValidateAccessTokenAsync(accessToken, share.ShareCode, share.AccessVersion, DateTime.UtcNow))
        {
            throw new ExplicitException("需要分享密码");
        }
        return share;
    }

    private async Task<(ShareEntity Share, FileEntity File)> GetPublicFileTargetAsync(string shareCode, string? accessToken)
    {
        var share = await GetPublicShareAsync(shareCode, accessToken, ShareTargetType.File);
        var file = await fileService.GetByIdAsync(share.TargetId);
        if (!ShareTargetPolicy.CanShareFile(file, share.TenantId, share.CreateUserId))
        {
            throw ShareUnavailable();
        }
        return (share, file);
    }

    private async Task<bool> IsTargetAvailableAsync(ShareEntity share)
    {
        return share.TargetType switch
        {
            ShareTargetType.File => ShareTargetPolicy.CanShareFile(await fileService.GetByIdAsync(share.TargetId), share.TenantId, share.CreateUserId),
            ShareTargetType.Note => ShareTargetPolicy.CanShareNote(await noteRepository.GetByIdAsync(share.TargetId), share.TenantId, share.CreateUserId),
            _ => false
        };
    }

    private async Task<string> GetOwnerNameAsync(ShareEntity share)
    {
        var owner = await userRepository.GetAsync(entity =>
            entity.Id == share.CreateUserId && entity.TenantId == share.TenantId && !entity.IsDelete);
        return ShareOwnerDisplayName.Resolve(owner);
    }

    private async Task<PublicShareStream> OpenFileAsync(FileEntity file)
    {
        try
        {
            var stream = await fileStorageFactory.GetFileStorage(file.StoreType).DownloadAsync(file.Path);
            return new PublicShareStream(stream, file.ContentType, file.Name);
        }
        catch (FileNotFoundException)
        {
            throw ShareUnavailable();
        }
    }

    private static bool HasPassword(ShareEntity share) => !string.IsNullOrWhiteSpace(share.PasswordCiphertext);

    private void ApplyPassword(ShareEntity share, string? password)
    {
        var normalizedPassword = password?.Trim() ?? string.Empty;
        if (normalizedPassword.Length == 0)
        {
            if (share.PasswordCiphertext != null)
            {
                share.PasswordCiphertext = null;
                share.AccessVersion++;
            }
            return;
        }
        if (normalizedPassword.Length < 4)
        {
            throw new ExplicitException("分享密码长度不能少于4位");
        }
        if (share.PasswordCiphertext != null && passwordProtector.IsMatch(normalizedPassword, share.PasswordCiphertext))
        {
            return;
        }
        share.PasswordCiphertext = passwordProtector.Encrypt(normalizedPassword);
        share.AccessVersion++;
    }

    private ShareConfigDto ToConfig(ShareEntity? share)
    {
        var config = ShareLifecycle.ToConfig(share);
        if (share != null && HasPassword(share))
        {
            config.Password = passwordProtector.Decrypt(share.PasswordCiphertext!);
        }
        return config;
    }

    private static ExplicitException ShareUnavailable() => new("分享不存在或已失效");

    private Task<ShareEntity?> GetOwnedShareAsync(ShareTargetType targetType, long targetId)
    {
        return shareRepository.GetAsync(entity =>
            entity.TenantId == TenantContextHolder.TenantId &&
            entity.CreateUserId == TenantContextHolder.UserId &&
            entity.TargetType == targetType &&
            entity.TargetId == targetId);
    }

    private async Task<ShareEntity> GetRequiredOwnedShareAsync(ShareTargetType targetType, long targetId)
    {
        var share = await GetOwnedShareAsync(targetType, targetId);
        if (share == null || share.Id < 1 || share.IsDelete)
        {
            throw new ExplicitException("分享配置不存在");
        }
        return share;
    }

    private async Task PersistAsync(ShareEntity share)
    {
        var count = await shareRepository.UpdateAsync(
            share,
            entity => new
            {
                entity.ShareCode,
                entity.ExpiresAt,
                entity.PasswordCiphertext,
                entity.IsEnabled,
                entity.AccessVersion,
                entity.IsDelete
            },
            entity => entity.Id == share.Id &&
                      entity.TenantId == TenantContextHolder.TenantId &&
                      entity.CreateUserId == TenantContextHolder.UserId);
        if (count < 1)
        {
            throw new ExplicitException("保存分享失败");
        }
    }
}