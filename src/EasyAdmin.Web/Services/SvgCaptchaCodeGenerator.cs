using System.Security.Cryptography;
using System.Text;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Models;

namespace EasyAdmin.Web.Services;

/// <summary>
/// SVG验证码生成器
/// </summary>
public class SvgCaptchaCodeGenerator : ICaptchaCodeGenerator
{
    /// <summary>
    /// 可用字符
    /// </summary>
    public const string AllowedCharacters = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    /// <summary>
    /// 生成验证码
    /// </summary>
    public CaptchaCode Generate(int codeLength)
    {
        var code = new string(Enumerable.Range(0, codeLength)
            .Select(_ => AllowedCharacters[RandomNumberGenerator.GetInt32(AllowedCharacters.Length)])
            .ToArray());

        var width = Math.Max(120, codeLength * 30);
        const int height = 48;
        var svg = new StringBuilder()
            .Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\">")
            .Append("<rect width=\"100%\" height=\"100%\" rx=\"4\" fill=\"#f2f6fc\"/>");

        for (var index = 0; index < 4; index++)
        {
            svg.Append($"<line x1=\"{Next(width)}\" y1=\"{Next(height)}\" x2=\"{Next(width)}\" y2=\"{Next(height)}\" stroke=\"{Color()}\" stroke-width=\"1\" opacity=\"0.65\"/>");
        }

        for (var index = 0; index < 24; index++)
        {
            svg.Append($"<circle cx=\"{Next(width)}\" cy=\"{Next(height)}\" r=\"1\" fill=\"{Color()}\" opacity=\"0.55\"/>");
        }

        for (var index = 0; index < code.Length; index++)
        {
            var x = 17 + index * 30 + RandomNumberGenerator.GetInt32(-2, 3);
            var y = 33 + RandomNumberGenerator.GetInt32(-3, 4);
            var angle = RandomNumberGenerator.GetInt32(-18, 19);
            svg.Append($"<text x=\"{x}\" y=\"{y}\" transform=\"rotate({angle} {x} {y})\" font-family=\"Arial,sans-serif\" font-size=\"27\" font-weight=\"700\" fill=\"{Color()}\">{code[index]}</text>");
        }

        svg.Append("</svg>");
        var image = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg.ToString()));
        return new CaptchaCode(code, image);
    }

    private static int Next(int maxValue) => RandomNumberGenerator.GetInt32(maxValue);

    private static string Color()
    {
        var red = RandomNumberGenerator.GetInt32(35, 125);
        var green = RandomNumberGenerator.GetInt32(55, 135);
        var blue = RandomNumberGenerator.GetInt32(75, 155);
        return $"rgb({red},{green},{blue})";
    }
}
