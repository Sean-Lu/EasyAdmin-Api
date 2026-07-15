using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 匿名分享规则
/// </summary>
public static class SharePublicAccess
{
    public static bool IsUnavailable(ShareEntity? share, DateTime now)
    {
        return share == null
               || share.Id < 1
               || share.IsDelete
               || !share.IsEnabled
               || (share.ExpiresAt.HasValue && share.ExpiresAt.Value <= now);
    }

    public static bool CanAccessNoteImage(
        NoteEntity? note,
        FileEntity? image,
        IReadOnlySet<long> referencedImageIds)
    {
        return note is { Id: > 0, IsDelete: false }
               && image is { Id: > 0, IsDelete: false, BizType: FileBizType.NoteImage }
               && note.TenantId == image.TenantId
               && note.UserId == image.CreateUserId
               && referencedImageIds.Contains(image.Id);
    }
}