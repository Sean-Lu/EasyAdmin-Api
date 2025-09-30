using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Application.Contracts;

public interface IUserService
{
    Task<bool> AddAsync(UserDto dto);
    Task<bool> DeleteByIdAsync(long id);
    Task<bool> DeleteByIdsAsync(List<long> ids);
    Task<bool> UpdateAsync(UserDto dto);
    Task<bool> UpdateStateAsync(long id, CommonState state);
    /// <summary>
    /// 更新用户最后登录时间
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="lastLoginTime">最后登录时间</param>
    /// <returns></returns>
    Task<bool> UpdateLastLoginTimeAsync(long id, DateTime lastLoginTime);
    Task<PageQueryResult<UserEntity>> PageAsync(UserPageReqDto request);
    Task<List<UserEntity>> GetAllUserAsync();
    Task<UserEntity> GetByIdAsync(long id);
    Task<UserEntity?> GetAsync(string username, string password);
    /// <summary>
    /// 校验密码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<bool> CheckPasswordAsync(long userId, string password);
    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="oldPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    Task<bool> ChangePasswordAsync(long userId, string oldPassword, string newPassword);
    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    Task<bool> ResetPasswordAsync(long userId, string? newPassword = null);
}