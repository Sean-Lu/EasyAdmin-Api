using System.Text.RegularExpressions;

namespace EasyAdmin.Application.Services;

public static partial class NoteMarkdownPackageHelper
{
    public static bool IsSafeEntryPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path) || path.Contains(':'))
        {
            return false;
        }
        return !path.Replace('\\', '/').Split('/').Any(part => part == "..");
    }

    public static IReadOnlyList<string> ExtractRelativeImagePaths(string markdown)
    {
        return RelativeImageRegex().Matches(markdown ?? string.Empty)
            .Select(match => match.Groups["path"].Value.Replace('\\', '/'))
            .Where(IsSafeEntryPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string RewriteRelativeImages(string markdown, IReadOnlyDictionary<string, long> replacements)
    {
        return RelativeImageRegex().Replace(markdown ?? string.Empty, match =>
        {
            var path = match.Groups["path"].Value.Replace('\\', '/');
            return replacements.TryGetValue(path, out var id) ? $"![{match.Groups["alt"].Value}](note-file:{id})" : match.Value;
        });
    }

    public static bool IsSupportedImageContent(string extension, ReadOnlySpan<byte> content)
    {
        return extension.ToLowerInvariant() switch
        {
            ".png" => content.StartsWith(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }),
            ".jpg" or ".jpeg" => content.StartsWith(new byte[] { 0xff, 0xd8, 0xff }),
            ".gif" => content.StartsWith("GIF87a"u8) || content.StartsWith("GIF89a"u8),
            ".webp" => content.Length >= 12 && content[..4].SequenceEqual("RIFF"u8) && content.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    [GeneratedRegex("!\\[(?<alt>[^\\]]*)\\]\\((?<path>[^)]+)\\)", RegexOptions.IgnoreCase)]
    private static partial Regex RelativeImageRegex();
}
