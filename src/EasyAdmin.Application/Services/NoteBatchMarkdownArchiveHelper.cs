using System.IO.Compression;
using System.Text;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 笔记批量Markdown归档工具
/// </summary>
public static class NoteBatchMarkdownArchiveHelper
{
    /// <summary>
    /// 构建批量Markdown归档
    /// </summary>
    public static byte[] Build(IEnumerable<(byte[] Content, string FileName)> files, bool includeImages)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            var usedNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                if (includeImages)
                {
                    AddPackage(archive, file, usedNames);
                }
                else
                {
                    AddFile(archive, file.Content, GetUniqueName(file.FileName, usedNames));
                }
            }
        }
        return output.ToArray();
    }

    private static void AddPackage(
        ZipArchive archive,
        (byte[] Content, string FileName) file,
        Dictionary<string, int> usedNames)
    {
        var directoryName = GetUniqueName(Path.GetFileNameWithoutExtension(file.FileName), usedNames);
        using var package = new ZipArchive(new MemoryStream(file.Content), ZipArchiveMode.Read);
        foreach (var packageEntry in package.Entries.Where(entry => !string.IsNullOrEmpty(entry.Name)))
        {
            var entryName = $"{directoryName}/{packageEntry.FullName.Replace('\\', '/')}";
            var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
            using var source = packageEntry.Open();
            using var target = entry.Open();
            source.CopyTo(target);
        }
    }

    private static void AddFile(ZipArchive archive, byte[] content, string entryName)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
        using var target = entry.Open();
        target.Write(content);
    }

    private static string GetUniqueName(string name, Dictionary<string, int> usedNames)
    {
        if (!usedNames.TryGetValue(name, out var count))
        {
            usedNames[name] = 1;
            return name;
        }

        count++;
        usedNames[name] = count;
        var extension = Path.GetExtension(name);
        var baseName = Path.GetFileNameWithoutExtension(name);
        return $"{baseName} ({count}){extension}";
    }
}
