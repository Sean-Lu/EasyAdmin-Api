namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 笔记Markdown服务
/// </summary>
public interface INoteMarkdownService
{
    string Render(string? markdown);

    IReadOnlyList<long> ExtractImageFileIds(string? markdown);

    string RewriteImageReferences(string markdown, IReadOnlyDictionary<long, string> replacements);
}
