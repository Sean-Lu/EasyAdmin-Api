using System.Net;
using System.Text.RegularExpressions;

namespace EasyAdmin.Application.Services;

public static partial class NoteContentHelper
{
    public static string SanitizeHtml(string? html)
    {
        var value = html ?? string.Empty;
        value = ScriptRegex().Replace(value, string.Empty);
        value = EventAttributeRegex().Replace(value, string.Empty);
        value = JavascriptUrlRegex().Replace(value, "$1#");
        return value;
    }

    public static string ExtractText(string? html)
    {
        var withoutScripts = ScriptRegex().Replace(html ?? string.Empty, string.Empty);
        var withoutTags = TagRegex().Replace(withoutScripts, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return NormalizeSpaces(decoded);
    }

    public static string CreateSummary(string? text, int maxLength = 120)
    {
        var value = NormalizeSpaces(text ?? string.Empty);
        if (value.Length <= maxLength)
        {
            return value;
        }
        return value[..maxLength];
    }

    public static List<long> ExtractImageFileIds(string? html)
    {
        return ImageFileIdRegex()
            .Matches(html ?? string.Empty)
            .Select(match => long.TryParse(match.Groups["id"].Value, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    private static string NormalizeSpaces(string value)
    {
        return string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    [GeneratedRegex("<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptRegex();

    [GeneratedRegex("\\s+on[a-z]+\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^\\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EventAttributeRegex();

    [GeneratedRegex("(href|src)\\s*=\\s*(\"|')?\\s*javascript:[^\"'\\s>]+(\"|')?", RegexOptions.IgnoreCase)]
    private static partial Regex JavascriptUrlRegex();

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex TagRegex();

    [GeneratedRegex("data-file-id\\s*=\\s*(\"|')(?<id>\\d+)\\1", RegexOptions.IgnoreCase)]
    private static partial Regex ImageFileIdRegex();
}
