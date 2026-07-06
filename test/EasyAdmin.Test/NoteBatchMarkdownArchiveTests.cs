using EasyAdmin.Application.Services;
using System.IO.Compression;
using System.Text;

namespace EasyAdmin.Test;

[TestClass]
public class NoteBatchMarkdownArchiveTests
{
    [TestMethod]
    public void Build_PutsMarkdownFilesAtArchiveRoot()
    {
        var files = new[]
        {
            (Encoding.UTF8.GetBytes("# 第一篇"), "笔记.md"),
            (Encoding.UTF8.GetBytes("# 第二篇"), "笔记.md")
        };

        var content = NoteBatchMarkdownArchiveHelper.Build(files, false);

        using var archive = OpenArchive(content);
        CollectionAssert.AreEqual(new[] { "笔记.md", "笔记 (2).md" }, archive.Entries.Select(entry => entry.FullName).ToArray());
    }

    [TestMethod]
    public void Build_PutsEachResourcePackageInItsOwnDirectory()
    {
        var files = new[]
        {
            (CreatePackage(("笔记.md", "# 标题"), ("images/image-1.png", "image")), "笔记.zip")
        };

        var content = NoteBatchMarkdownArchiveHelper.Build(files, true);

        using var archive = OpenArchive(content);
        CollectionAssert.AreEqual(
            new[] { "笔记/笔记.md", "笔记/images/image-1.png" },
            archive.Entries.Select(entry => entry.FullName).ToArray());
    }

    private static ZipArchive OpenArchive(byte[] content) =>
        new(new MemoryStream(content), ZipArchiveMode.Read);

    private static byte[] CreatePackage(params (string Name, string Content)[] files)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Name);
                using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
                writer.Write(file.Content);
            }
        }
        return output.ToArray();
    }
}
