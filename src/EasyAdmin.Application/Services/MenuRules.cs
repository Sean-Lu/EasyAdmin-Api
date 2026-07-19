using System.Text.RegularExpressions;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Helper;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

public static partial class MenuRules
{
    private static readonly Dictionary<string, string> UnSupportIframeLinks = new(StringComparer.OrdinalIgnoreCase)
    {
        { "github.com", "GitHub" },
        { "gitee.com", "Gitee" }
    };

    public static void NormalizeAndValidate(MenuEntity entity)
    {
        entity.Path = Normalize(entity.Path);
        entity.OutLink = Normalize(entity.OutLink);

        switch (entity.Type)
        {
            case MenuType.Directory:
                entity.Path = null;
                entity.OutLink = null;
                entity.OutLinkOpenType = null;
                return;
            case MenuType.Internal:
                entity.OutLink = null;
                entity.OutLinkOpenType = null;
                ValidateInternalPath(entity.Path);
                return;
            case MenuType.External:
                ValidateExternal(entity);
                return;
            default:
                throw new ExplicitException("菜单类型无效");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidateInternalPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith('/') || path.StartsWith("/link/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExplicitException("内部菜单路由格式无效");
        }
    }

    private static void ValidateExternal(MenuEntity entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Path) || !ExternalPathRegex().IsMatch(entity.Path))
        {
            throw new ExplicitException("外链菜单路由格式无效");
        }
        if (string.IsNullOrWhiteSpace(entity.OutLink) || !UrlHelper.IsValidUrl(entity.OutLink))
        {
            throw new ExplicitException("外部链接格式无效");
        }
        if (!UrlHelper.IsAllowedHost(entity.OutLink))
        {
            throw new ExplicitException($"不允许访问的外部链接域名: {UrlHelper.GetHost(entity.OutLink)}");
        }
        if (!entity.OutLinkOpenType.HasValue || !Enum.IsDefined(entity.OutLinkOpenType.Value))
        {
            throw new ExplicitException("请选择外链打开方式");
        }
        if (entity.OutLinkOpenType == OutLinkOpenType.Inline)
        {
            var host = UrlHelper.GetHost(entity.OutLink);
            if (UnSupportIframeLinks.TryGetValue(host, out var name))
            {
                throw new ExplicitException($"{name}不支持iframe内嵌打开，请使用新标签页方式");
            }
        }
    }

    [GeneratedRegex("^/link/[A-Za-z0-9][A-Za-z0-9_-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ExternalPathRegex();
}