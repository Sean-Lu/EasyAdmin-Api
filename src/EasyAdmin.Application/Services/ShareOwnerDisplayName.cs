using EasyAdmin.Domain.Entities;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 分享者展示名称
/// </summary>
public static class ShareOwnerDisplayName
{
    public static string Resolve(UserEntity? user)
    {
        if (!string.IsNullOrWhiteSpace(user?.NickName))
        {
            return user.NickName.Trim();
        }
        if (!string.IsNullOrWhiteSpace(user?.UserName))
        {
            return user.UserName.Trim();
        }
        return "分享者";
    }
}