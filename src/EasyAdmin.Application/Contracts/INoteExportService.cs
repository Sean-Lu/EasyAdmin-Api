using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

public interface INoteExportService
{
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(NoteDto note, string exportType);

    Task<(byte[] Content, string ContentType, string FileName)> BatchExportAsync(IEnumerable<NoteDto> notes, string exportType);
}
