using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

public interface INoteExportService
{
    Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(NoteDto note, string exportType);
}
