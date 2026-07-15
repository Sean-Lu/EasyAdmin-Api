using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 分享生命周期规则
/// </summary>
public static class ShareLifecycle
{
    public static void ValidateExpiry(DateTime? expiresAt, DateTime now)
    {
        if (expiresAt.HasValue && expiresAt.Value <= now)
        {
            throw new ExplicitException("分享有效期必须晚于当前时间");
        }
    }

    public static void ApplyEnabled(ShareEntity share, bool isEnabled)
    {
        if (share.IsEnabled == isEnabled)
        {
            return;
        }
        share.IsEnabled = isEnabled;
        share.AccessVersion++;
    }

    public static void RegenerateCode(ShareEntity share)
    {
        share.ShareCode = ShareSecurity.CreateCode();
        share.AccessVersion++;
    }

    public static ShareConfigDto ToConfig(ShareEntity? share)
    {
        if (share == null || share.Id < 1 || share.IsDelete)
        {
            return new ShareConfigDto();
        }
        return new ShareConfigDto
        {
            Exists = true,
            ShareCode = share.ShareCode,
            IsEnabled = share.IsEnabled,
            ExpiresAt = share.ExpiresAt,
            HasPassword = !string.IsNullOrWhiteSpace(share.PasswordCiphertext)
        };
    }
}