using EasyAdmin.Application.Dtos;

namespace EasyAdmin.Application.Contracts;

public interface INotePasswordService
{
    Task<NotePasswordStatusDto> GetStatusAsync();
    Task<bool> SetAsync(NotePasswordSetDto dto);
    Task<bool> ChangeAsync(NotePasswordChangeDto dto);
    Task<NotePasswordVerifyResultDto> VerifyAsync(NotePasswordVerifyDto dto);
    Task<bool> ResetAsync(NotePasswordVerifyDto dto);
    Task<bool> ValidateUnlockTokenAsync(string? unlockToken);
}
