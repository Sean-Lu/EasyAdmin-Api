using EasyAdmin.Application.Services;

namespace EasyAdmin.Test;

[TestClass]
public class NoteContentHelperTests
{
    [TestMethod]
    public void SanitizeHtml_RemovesScript()
    {
        var html = "<p>hello</p><script>alert(1)</script>";
        var result = NoteContentHelper.SanitizeHtml(html);
        Assert.IsTrue(result.Contains("<p>hello</p>"));
        Assert.IsFalse(result.Contains("script"));
    }

    [TestMethod]
    public void ExtractText_ReturnsPlainText()
    {
        var result = NoteContentHelper.ExtractText("<h1>标题</h1><p>正文</p>");
        Assert.AreEqual("标题 正文", result);
    }

    [TestMethod]
    public void CreateSummary_TruncatesLongText()
    {
        var text = new string('中', 140);
        var result = NoteContentHelper.CreateSummary(text, 100);
        Assert.AreEqual(100, result.Length);
    }

    [TestMethod]
    public void ExtractImageFileIds_ReturnsDistinctIds()
    {
        var html = "<p>正文</p><img src=\"\" data-file-id=\"12\" /><img data-file-id='34' /><img data-file-id=\"12\" />";
        var result = NoteContentHelper.ExtractImageFileIds(html);
        CollectionAssert.AreEqual(new List<long> { 12, 34 }, result);
    }
}
