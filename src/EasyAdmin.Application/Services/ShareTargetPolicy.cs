using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 分享目标规则
/// </summary>
public static class ShareTargetPolicy
{
    public static bool CanShareFile(FileEntity? file, long tenantId, long userId)
    {
        return file is { Id: > 0, IsDelete: false, BizType: FileBizType.Normal }
               && file.TenantId == tenantId
               && file.CreateUserId == userId;
    }

    public static bool CanShareNote(NoteEntity? note, long tenantId, long userId)
    {
        return note is { Id: > 0, IsDelete: false }
               && note.TenantId == tenantId
               && note.UserId == userId;
    }
}