using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Storage;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 笔记导出服务
/// </summary>
public partial class NoteExportService(
    IFileService fileService,
    IFileStorageFactory fileStorageFactory,
    INotePdfRenderer notePdfRenderer
    ) : INoteExportService
{
    public async Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(NoteDto note, string exportType)
    {
        var type = (exportType ?? "html").Trim().ToLowerInvariant();
        var html = await BuildHtmlAsync(note);
        return type switch
        {
            "doc" or "word" => (Encoding.UTF8.GetBytes(html), "application/msword", $"{NormalizeFileName(note.Title)}.doc"),
            "pdf" => (await notePdfRenderer.RenderAsync(html), "application/pdf", $"{NormalizeFileName(note.Title)}.pdf"),
            _ => (Encoding.UTF8.GetBytes(html), "text/html", $"{NormalizeFileName(note.Title)}.html")
        };
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> BatchExportAsync(IEnumerable<NoteDto> notes, string exportType)
    {
        await using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            var usedNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var note in notes)
            {
                var file = await ExportAsync(note, exportType);
                var entryName = GetUniqueEntryName(file.FileName, usedNames);
                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(file.Content);
            }
        }
        return (memoryStream.ToArray(), "application/zip", "我的笔记.zip");
    }

    private async Task<string> BuildHtmlAsync(NoteDto note)
    {
        var title = WebUtility.HtmlEncode(note.Title);
        var tags = note.Tags.Any() ? string.Join(" ", note.Tags.Select(tag => $"#{WebUtility.HtmlEncode(tag.Name)}")) : string.Empty;
        var contentHtml = await EmbedImagesAsync(note.ContentHtml ?? string.Empty);
        return $$"""
<!doctype html>
<html>
<head>
  <meta charset="utf-8">
  <title>{{title}}</title>
  <style>
    body { font-family: "Microsoft YaHei", Arial, sans-serif; line-height: 1.7; color: #1f2937; padding: 32px; }
    h1 { font-size: 28px; margin: 0 0 12px; }
    .meta { color: #6b7280; margin-bottom: 24px; }
    img { max-width: 100%; }
  </style>
</head>
<body>
  <h1>{{title}}</h1>
  <div class="meta">{{WebUtility.HtmlEncode(note.CategoryName ?? "未分类")}} {{tags}}</div>
  {{contentHtml}}
</body>
</html>
""";
    }

    private async Task<string> EmbedImagesAsync(string html)
    {
        var replacements = new Dictionary<string, string>();
        foreach (Match match in ImageTagRegex().Matches(html))
        {
            if (!long.TryParse(match.Groups["id"].Value, out var fileId) || fileId < 1)
            {
                continue;
            }

            var dataUri = await GetImageDataUriAsync(fileId);
            if (string.IsNullOrWhiteSpace(dataUri))
            {
                continue;
            }

            var imageTag = match.Value;
            var embeddedImageTag = SrcAttributeRegex().IsMatch(imageTag)
                ? SrcAttributeRegex().Replace(imageTag, $" src=\"{dataUri}\"", 1)
                : InsertSrcAttribute(imageTag, dataUri);
            replacements[imageTag] = NormalizeImageSizeAttributes(embeddedImageTag);
        }

        foreach (var replacement in replacements)
        {
            html = html.Replace(replacement.Key, replacement.Value);
        }
        return html;
    }

    private static string InsertSrcAttribute(string imageTag, string dataUri)
    {
        var insertIndex = imageTag.EndsWith("/>", StringComparison.Ordinal) ? imageTag.Length - 2 : imageTag.Length - 1;
        return imageTag.Insert(insertIndex, $" src=\"{dataUri}\"");
    }

    private static string NormalizeImageSizeAttributes(string imageTag)
    {
        var styleMatch = StyleAttributeRegex().Match(imageTag);
        if (!styleMatch.Success)
        {
            return imageTag;
        }

        var style = styleMatch.Groups["style"].Value;
        var width = TryGetPixelStyleValue(style, "width");
        var height = TryGetPixelStyleValue(style, "height");
        if (width == null && height == null)
        {
            return imageTag;
        }

        if (width != null)
        {
            imageTag = UpsertAttribute(imageTag, "width", width.Value.ToString());
        }
        if (height != null)
        {
            imageTag = UpsertAttribute(imageTag, "height", height.Value.ToString());
        }
        return imageTag;
    }

    private static int? TryGetPixelStyleValue(string style, string name)
    {
        var match = Regex.Match(style, $@"(?:^|;)\s*{Regex.Escape(name)}\s*:\s*(?<value>\d+(?:\.\d+)?)px\s*(?:;|$)", RegexOptions.IgnoreCase);
        return match.Success && decimal.TryParse(match.Groups["value"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? (int)Math.Round(value) : null;
    }

    private static string UpsertAttribute(string imageTag, string name, string value)
    {
        var regex = new Regex($@"\s+{Regex.Escape(name)}\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)", RegexOptions.IgnoreCase);
        if (regex.IsMatch(imageTag))
        {
            return regex.Replace(imageTag, $" {name}=\"{value}\"", 1);
        }

        var insertIndex = imageTag.EndsWith("/>", StringComparison.Ordinal) ? imageTag.Length - 2 : imageTag.Length - 1;
        return imageTag.Insert(insertIndex, $" {name}=\"{value}\"");
    }

    private async Task<string?> GetImageDataUriAsync(long fileId)
    {
        var file = await fileService.GetByIdAsync(fileId);
        if (file == null || file.Id < 1 || string.IsNullOrWhiteSpace(file.Path)
            || file.BizType != FileBizType.NoteImage
            || file.TenantId != TenantContextHolder.TenantId
            || file.CreateUserId != TenantContextHolder.UserId)
        {
            return null;
        }

        await using var stream = await fileStorageFactory.GetFileStorage(file.StoreType).DownloadAsync(file.Path);
        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        return $"data:{contentType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
    }

    private static string NormalizeFileName(string? fileName)
    {
        var value = string.IsNullOrWhiteSpace(fileName) ? "我的笔记" : fileName.Trim();
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }
        return value;
    }

    private static string GetUniqueEntryName(string fileName, Dictionary<string, int> usedNames)
    {
        var name = NormalizeFileName(Path.GetFileNameWithoutExtension(fileName));
        var extension = Path.GetExtension(fileName);
        var key = $"{name}{extension}";
        if (!usedNames.TryGetValue(key, out var count))
        {
            usedNames[key] = 1;
            return key;
        }

        count++;
        usedNames[key] = count;
        return $"{name}({count}){extension}";
    }

    [GeneratedRegex("<img\\b[^>]*data-file-id\\s*=\\s*(\"|')(?<id>\\d+)\\1[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ImageTagRegex();

    [GeneratedRegex("\\s+src\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^\\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex SrcAttributeRegex();

    [GeneratedRegex("\\s+style\\s*=\\s*(\"(?<style>[^\"]*)\"|'(?<style>[^']*)')", RegexOptions.IgnoreCase)]
    private static partial Regex StyleAttributeRegex();
}
