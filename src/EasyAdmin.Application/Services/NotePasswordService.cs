using System.Collections.Concurrent;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 笔记密码服务
/// </summary>
public class NotePasswordService(
    IUserNotePasswordRepository userNotePasswordRepository
    ) : INotePasswordService
{
    private const int UnlockExpireMinutes = 30;
    private static readonly TimeSpan UnlockTokenExpire = TimeSpan.FromMinutes(UnlockExpireMinutes);
    private static readonly ConcurrentDictionary<string, (long UserId, DateTime ExpireTime)> UnlockTokens = new();

    public async Task<NotePasswordStatusDto> GetStatusAsync()
    {
        var entity = await GetCurrentAsync();
        return new NotePasswordStatusDto { HasPassword = entity != null && entity.Id > 0 };
    }

    public async Task<bool> SetAsync(NotePasswordSetDto dto)
    {
        var password = dto.Password?.Trim() ?? string.Empty;
        if (password.Length < 4)
        {
            throw new ExplicitException("密码长度不能少于4位");
        }

        var current = await GetCurrentAsync();
        if (current != null && current.Id > 0)
        {
            throw new ExplicitException("笔记密码已设置");
        }

        var (hash, salt) = NotePasswordHasher.Hash(password);
        return await userNotePasswordRepository.AddAsync(new UserNotePasswordEntity
        {
            UserId = TenantContextHolder.UserId,
            PasswordHash = hash,
            PasswordSalt = salt
        });
    }

    public async Task<bool> ChangeAsync(NotePasswordChangeDto dto)
    {
        var current = await GetRequiredAsync();
        if (!NotePasswordHasher.Verify(dto.OldPassword ?? string.Empty, current.PasswordHash, current.PasswordSalt))
        {
            throw new ExplicitException("原密码不正确");
        }

        var newPassword = dto.NewPassword?.Trim() ?? string.Empty;
        if (newPassword.Length < 4)
        {
            throw new ExplicitException("新密码长度不能少于4位");
        }

        var (hash, salt) = NotePasswordHasher.Hash(newPassword);
        return await userNotePasswordRepository.UpdateAsync(new UserNotePasswordEntity
        {
            PasswordHash = hash,
            PasswordSalt = salt
        }, entity => new { entity.PasswordHash, entity.PasswordSalt },
            entity => entity.UserId == TenantContextHolder.UserId && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }

    public async Task<NotePasswordVerifyResultDto> VerifyAsync(NotePasswordVerifyDto dto)
    {
        var current = await GetRequiredAsync();
        if (!NotePasswordHasher.Verify(dto.Password ?? string.Empty, current.PasswordHash, current.PasswordSalt))
        {
            throw new ExplicitException("密码不正确");
        }

        var token = Guid.NewGuid().ToString("N");
        UnlockTokens[token] = (TenantContextHolder.UserId, DateTime.Now.Add(UnlockTokenExpire));
        return new NotePasswordVerifyResultDto
        {
            UnlockToken = token,
            ExpireMinutes = UnlockExpireMinutes
        };
    }

    public async Task<bool> ResetAsync(NotePasswordVerifyDto dto)
    {
        var current = await GetRequiredAsync();
        if (!NotePasswordHasher.Verify(dto.Password ?? string.Empty, current.PasswordHash, current.PasswordSalt))
        {
            throw new ExplicitException("密码不正确");
        }

        ClearUserTokens(TenantContextHolder.UserId);
        return await userNotePasswordRepository.DeleteByIdAsync(current.Id);
    }

    public Task<bool> ValidateUnlockTokenAsync(string? unlockToken)
    {
        if (string.IsNullOrWhiteSpace(unlockToken))
        {
            return Task.FromResult(false);
        }

        if (!UnlockTokens.TryGetValue(unlockToken, out var tokenInfo))
        {
            return Task.FromResult(false);
        }

        if (tokenInfo.ExpireTime < DateTime.Now)
        {
            UnlockTokens.TryRemove(unlockToken, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(tokenInfo.UserId == TenantContextHolder.UserId);
    }

    private async Task<UserNotePasswordEntity?> GetCurrentAsync()
    {
        return await userNotePasswordRepository.GetAsync(entity =>
            entity.UserId == TenantContextHolder.UserId &&
            entity.TenantId == TenantContextHolder.TenantId &&
            !entity.IsDelete);
    }

    private async Task<UserNotePasswordEntity> GetRequiredAsync()
    {
        var current = await GetCurrentAsync();
        if (current == null || current.Id < 1)
        {
            throw new ExplicitException("请先设置笔记密码");
        }
        return current;
    }

    private static void ClearUserTokens(long userId)
    {
        foreach (var pair in UnlockTokens.Where(pair => pair.Value.UserId == userId).ToList())
        {
            UnlockTokens.TryRemove(pair.Key, out _);
        }
    }
}
