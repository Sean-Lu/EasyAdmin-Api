using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

/// <summary>
/// 笔记标签服务接口
/// </summary>
public interface INoteTagService
{
    Task<List<NoteTagDto>> GetByUserIdAsync();
    Task<List<NoteTagDto>> SuggestAsync(string? keyword);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteUnusedAsync();
}
