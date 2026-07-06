using EasyAdmin.Application.Services;

namespace EasyAdmin.Test;

[TestClass]
public class NoteMarkdownPackageHelperTests
{
    [DataTestMethod]
    [DataRow("images/a.png", true)]
    [DataRow("../a.png", false)]
    [DataRow("C:/a.png", false)]
    [DataRow("/a.png", false)]
    public void IsSafeEntryPath_ValidatesArchivePaths(string path, bool expected)
    {
        Assert.AreEqual(expected, NoteMarkdownPackageHelper.IsSafeEntryPath(path));
    }

    [TestMethod]
    public void RewriteRelativeImages_RewritesMappedImagesOnly()
    {
        var result = NoteMarkdownPackageHelper.RewriteRelativeImages(
            "![a](images/a.png) ![b](https://example.com/b.png)",
            new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase) { ["images/a.png"] = 12 });

        Assert.AreEqual("![a](note-file:12) ![b](https://example.com/b.png)", result);
    }

    [TestMethod]
    public void IsSupportedImageContent_ValidatesFileSignature()
    {
        Assert.IsTrue(NoteMarkdownPackageHelper.IsSupportedImageContent(".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }));
        Assert.IsTrue(NoteMarkdownPackageHelper.IsSupportedImageContent(".jpg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }));
        Assert.IsFalse(NoteMarkdownPackageHelper.IsSupportedImageContent(".png", "not png"u8.ToArray()));
    }
}
