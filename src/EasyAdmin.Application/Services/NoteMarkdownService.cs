using EasyAdmin.Application.Contracts;
using Ganss.Xss;
using Markdig;
using System.Text.RegularExpressions;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 笔记Markdown服务
/// </summary>
public partial class NoteMarkdownService : INoteMarkdownService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public string Render(string? markdown)
    {
        var html = Markdown.ToHtml(markdown ?? string.Empty, Pipeline);
        html = ControlledImageHtmlRegex().Replace(html, match =>
            $"<img data-file-id=\"{match.Groups["id"].Value}\" alt=\"{match.Groups["alt"].Value}\">");
        html = ImageHtmlRegex().Replace(html, string.Empty);
        return CreateSanitizer().Sanitize(html);
    }

    public IReadOnlyList<long> ExtractImageFileIds(string? markdown)
    {
        return MarkdownImageRegex()
            .Matches(markdown ?? string.Empty)
            .Select(match => long.TryParse(match.Groups["id"].Value, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    public string RewriteImageReferences(string markdown, IReadOnlyDictionary<long, string> replacements)
    {
        return MarkdownImageRegex().Replace(markdown ?? string.Empty, match =>
        {
            if (!long.TryParse(match.Groups["id"].Value, out var id) || !replacements.TryGetValue(id, out var target))
            {
                return match.Value;
            }
            return $"![{match.Groups["alt"].Value}]({target})";
        });
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.UnionWith(new[]
        {
            "p", "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "pre", "code", "ul", "ol", "li",
            "table", "thead", "tbody", "tr", "th", "td", "del", "strong", "em", "a", "hr", "br", "img", "input"
        });
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.UnionWith(new[] { "href", "title", "alt", "data-file-id", "type", "checked", "disabled" });
        sanitizer.AllowDataAttributes = true;
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.UnionWith(new[] { "http", "https", "mailto" });
        sanitizer.AllowedCssProperties.Clear();
        return sanitizer;
    }

    [GeneratedRegex("<img\\s+src=\"note-file:(?<id>\\d+)\"\\s+alt=\"(?<alt>[^\"]*)\"\\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex ControlledImageHtmlRegex();

    [GeneratedRegex("<img\\b(?![^>]*\\bdata-file-id=)[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ImageHtmlRegex();

    [GeneratedRegex("!\\[(?<alt>[^\\]]*)\\]\\(note-file:(?<id>\\d+)\\)", RegexOptions.IgnoreCase)]
    private static partial Regex MarkdownImageRegex();
}
