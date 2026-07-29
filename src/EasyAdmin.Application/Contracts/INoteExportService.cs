using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 笔记导出服务接口
/// </summary>
public interface INoteExportService
{
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(NoteDto note, string exportType);

    Task<(byte[] Content, string ContentType, string FileName)> BatchExportAsync(IEnumerable<NoteDto> notes, string exportType);
}
