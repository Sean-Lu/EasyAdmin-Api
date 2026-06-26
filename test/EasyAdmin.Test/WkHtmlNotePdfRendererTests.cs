using System.Text;
using EasyAdmin.Application.Services;

namespace EasyAdmin.Test;

[TestClass]
public class WkHtmlNotePdfRendererTests
{
    [TestMethod]
    public async Task RenderAsync_ReturnsReadablePdfBytes()
    {
        var renderer = new WkHtmlNotePdfRenderer();

        var bytes = await renderer.RenderAsync("""
<!doctype html>
<html>
<head><meta charset="utf-8"><title>PDF</title></head>
<body><h1>我的笔记</h1><p>正文内容</p></body>
</html>
""");

        Assert.IsTrue(bytes.Length > 1000);
        Assert.AreEqual("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
        StringAssert.Contains(Encoding.ASCII.GetString(bytes[^32..]), "%%EOF");
    }
}
