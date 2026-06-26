using System.Text;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Infrastructure.Wrapper;
using WkHtmlToPdfDotNet;

namespace EasyAdmin.Application.Services;

public class WkHtmlNotePdfRenderer : INotePdfRenderer
{
    private static readonly Lazy<SynchronizedConverter> Converter = new(() => new SynchronizedConverter(new PdfTools()));

    public Task<byte[]> RenderAsync(string html)
    {
        try
        {
            var document = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 12, Right = 12, Bottom = 12, Left = 12 },
                    DPI = 96
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        Encoding = Encoding.UTF8,
                        WebSettings =
                        {
                            DefaultEncoding = "utf-8",
                            LoadImages = true,
                            Background = true,
                            PrintMediaType = true
                        },
                        LoadSettings =
                        {
                            BlockLocalFileAccess = true,
                            StopSlowScript = true
                        }
                    }
                }
            };

            return Task.FromResult(Converter.Value.Convert(document));
        }
        catch (Exception ex)
        {
            throw new ExplicitException($"PDF导出失败：{ex.Message}");
        }
    }
}
