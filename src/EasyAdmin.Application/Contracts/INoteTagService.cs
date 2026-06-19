using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

public interface INoteTagService
{
    Task<List<NoteTagDto>> GetByUserIdAsync();
    Task<List<NoteTagDto>> SuggestAsync(string? keyword);
    Task<bool> DeleteUnusedAsync();
}
