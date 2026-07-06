namespace EasyAdmin.Application.Dtos;

/// <summary>
/// Markdown导入结果
/// </summary>
public class NoteMarkdownImportDto
{
    public string Title { get; set; } = string.Empty;

    public string ContentMarkdown { get; set; } = string.Empty;

    public List<long> UploadedImageIds { get; set; } = new();
}
