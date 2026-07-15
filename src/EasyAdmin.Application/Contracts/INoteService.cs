using EasyAdmin.Application.Dtos;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface INoteService
{
    Task<PageQueryResult<NoteDto>> PageAsync(NotePageReqDto request);
    Task<NoteDto?> GetDetailAsync(long id, string? unlockToken);
    Task<NoteDto?> GetSharedDetailAsync(long id, long tenantId, long userId);
    Task<IReadOnlySet<long>> GetSharedImageIdsAsync(long id, long tenantId, long userId);
    Task<bool> AddAsync(NoteUpdateDto dto);
    Task<bool> UpdateAsync(NoteUpdateDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> BatchDeleteAsync(List<long> ids);
    Task<bool> DeleteImageFileAsync(long id);
    Task<bool> UpdateTopAsync(long id, bool isTop);
    Task<bool> MoveCategoryAsync(long id, long categoryId);
    Task<bool> MoveCategoryAsync(List<long> ids, long categoryId);
    Task<(byte[] Content, string ContentType, string FileName)> ExportMarkdownAsync(long id, string? unlockToken, bool includeImages);
    Task<(byte[] Content, string ContentType, string FileName)> BatchExportMarkdownAsync(IEnumerable<long> ids, string? unlockToken, bool includeImages);
    Task<NoteMarkdownImportDto> ImportMarkdownPackageAsync(Stream stream, string fileName);
}