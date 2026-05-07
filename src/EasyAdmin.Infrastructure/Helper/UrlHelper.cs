namespace EasyAdmin.Infrastructure.Helper;

public static class UrlHelper
{
    private static readonly HashSet<string> AllowedSchemes = new() { "http", "https" };
    private static readonly HashSet<string> AllowedHosts = new()
    { 
        "github.com", 
        "gitee.com"
    };

    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!AllowedSchemes.Contains(uri.Scheme.ToLower()))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(uri.Host))
        {
            return false;
        }

        return true;
    }

    public static bool IsAllowedHost(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        //return AllowedHosts.Contains(uri.Host.ToLower()) || IsInternalHost(uri.Host);
        return true;
    }

    private static bool IsInternalHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        var lowerHost = host.ToLower();
        return lowerHost.EndsWith(".local") || 
               lowerHost.EndsWith(".localhost") || 
               lowerHost == "localhost" ||
               lowerHost.StartsWith("127.") ||
               lowerHost.StartsWith("192.168.") ||
               lowerHost.StartsWith("10.") ||
               lowerHost.StartsWith("172.");
    }

    public static string GetHost(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return string.Empty;
        }
    }
}