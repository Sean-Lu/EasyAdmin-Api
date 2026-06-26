using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using System.IO.Compression;

namespace EasyAdmin.Test;

[TestClass]
public class NoteExportServiceTests
{
    [TestMethod]
    public async Task ExportAsync_ReturnsPdfFile_WhenExportTypeIsPdf()
    {
        var renderer = new StubNotePdfRenderer();
        var service = new NoteExportService(null!, null!, renderer);
        var note = new NoteDto
        {
            Title = "PDF/笔记",
            CategoryName = "默认分类",
            ContentHtml = "<p>正文内容</p>",
            Tags = [new NoteTagDto { Name = "标签" }]
        };

        var result = await service.ExportAsync(note, "pdf");

        CollectionAssert.AreEqual(StubNotePdfRenderer.PdfBytes, result.Content);
        Assert.AreEqual("application/pdf", result.ContentType);
        Assert.AreEqual("PDF_笔记.pdf", result.FileName);
        Assert.IsTrue(renderer.LastHtml?.Contains("<p>正文内容</p>"));
    }

    [TestMethod]
    public async Task BatchExportAsync_ReturnsZipFile_WithEachExportedNote()
    {
        var renderer = new StubNotePdfRenderer();
        var service = new NoteExportService(null!, null!, renderer);
        var notes = new[]
        {
            new NoteDto { Title = "同名笔记", ContentHtml = "<p>第一篇</p>" },
            new NoteDto { Title = "同名笔记", ContentHtml = "<p>第二篇</p>" }
        };

        var result = await service.BatchExportAsync(notes, "html");

        Assert.AreEqual("application/zip", result.ContentType);
        Assert.AreEqual("我的笔记.zip", result.FileName);
        using var zipStream = new MemoryStream(result.Content);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        Assert.AreEqual(2, archive.Entries.Count);
        Assert.AreEqual("同名笔记.html", archive.Entries[0].FullName);
        Assert.AreEqual("同名笔记(2).html", archive.Entries[1].FullName);

        await using var firstStream = archive.Entries[0].Open();
        using var reader = new StreamReader(firstStream);
        Assert.IsTrue((await reader.ReadToEndAsync()).Contains("<p>第一篇</p>"));
    }

    private sealed class StubNotePdfRenderer : INotePdfRenderer
    {
        public static readonly byte[] PdfBytes = [0x25, 0x50, 0x44, 0x46];
        public string? LastHtml { get; private set; }

        public Task<byte[]> RenderAsync(string html)
        {
            LastHtml = html;
            return Task.FromResult(PdfBytes);
        }
    }
}
