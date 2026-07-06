using EasyAdmin.Application.Services;

namespace EasyAdmin.Test;

[TestClass]
public class NoteMarkdownServiceTests
{
    private readonly NoteMarkdownService _service = new();

    [TestMethod]
    public void Render_SupportsGfmAndControlledImage()
    {
        var html = _service.Render("## 标题\n\n- [x] 完成\n\n![图](note-file:12)");

        StringAssert.Contains(html, "<h2>标题</h2>");
        StringAssert.Contains(html, "type=\"checkbox\"");
        StringAssert.Contains(html, "data-file-id=\"12\"");
    }

    [TestMethod]
    public void Render_RemovesRawHtmlDangerousUrlsAndExternalImages()
    {
        var html = _service.Render("<script>alert(1)</script>\n\n[x](javascript:alert(1))\n\n![x](https://example.com/a.png)");

        Assert.IsFalse(html.Contains("<script", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(html.Contains("javascript:", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(html.Contains("https://example.com/a.png", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void ExtractImageFileIds_ReturnsDistinctControlledIds()
    {
        var ids = _service.ExtractImageFileIds("![a](note-file:12) ![b](note-file:12) ![c](note-file:8)");

        CollectionAssert.AreEqual(new long[] { 12, 8 }, ids.ToArray());
    }

    [TestMethod]
    public void RewriteImageReferences_RewritesOnlyMappedControlledIds()
    {
        var markdown = "![a](note-file:12) ![b](note-file:8)";

        var result = _service.RewriteImageReferences(markdown, new Dictionary<long, string> { [12] = "images/a.png" });

        Assert.AreEqual("![a](images/a.png) ![b](note-file:8)", result);
    }
}
