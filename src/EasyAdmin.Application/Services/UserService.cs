using AutoMapper;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Utility.Security.Provider;
using EasyAdmin.Infrastructure.Const;

namespace EasyAdmin.Application.Services;

public class UserService(
    ILogger<UserService> logger,
    IMapper mapper,
    IUserRepository userRepository,
    IParamRepository paramRepository
    ) : IUserService
{
    public async Task<bool> AddAsync(UserDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.UserName) && await userRepository.ExistsAsync(entity => entity.UserName == dto.UserName && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId))
        {
            throw new ExplicitException("用户名称已存在");
        }
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && await userRepository.ExistsAsync(entity => entity.PhoneNumber == dto.PhoneNumber && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId))
        {
            throw new ExplicitException("手机号已存在");
        }
        if (!string.IsNullOrWhiteSpace(dto.Email) && await userRepository.ExistsAsync(entity => entity.Email == dto.Email && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId))
        {
            throw new ExplicitException("邮箱已存在");
        }

        var entity = mapper.Map<UserEntity>(dto);
        if (string.IsNullOrWhiteSpace(entity.NickName))
        {
            entity.NickName = entity.UserName;
        }
        if (entity.Role == UserRole.Unknown)
        {
            entity.Role = UserRole.User;// 默认赋予普通用户角色
        }
        if (string.IsNullOrEmpty(entity.Password))
        {
            var paramEntity = await paramRepository.GetAsync(c => c.ParamKey == ConfigConst.UserInitPassword && c.State == CommonState.Enable);
            var userInitPassword = paramEntity?.ParamValue;
            if (!string.IsNullOrEmpty(userInitPassword))
            {
                var hash = new HashCryptoProvider();
                entity.Password = hash.MD5(userInitPassword).ToLower();
            }
        }
        else
        {
            var hash = new HashCryptoProvider();
            entity.Password = hash.MD5(entity.Password).ToLower();
        }
        return await userRepository.AddAsync(entity);
    }

    public async Task<bool> DeleteByIdAsync(long id)
    {
        return await userRepository.DeleteByIdAsync(id);
    }
    public async Task<bool> DeleteByIdsAsync(List<long> ids)
    {
        return await userRepository.DeleteByIdsAsync(ids);
    }

    public async Task<bool> UpdateAsync(UserDto dto)
    {
        var entity = mapper.Map<UserEntity>(dto);
        return await userRepository.UpdateAsync(entity) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await userRepository.UpdateAsync(new UserEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id) > 0;
    }
    public async Task<bool> UpdateLastLoginTimeAsync(long id, DateTime lastLoginTime)
    {
        return await userRepository.UpdateAsync(new UserEntity { LastLoginTime = lastLoginTime }, entity => new { entity.LastLoginTime }, entity => entity.Id == id) > 0;
    }

    public async Task<PageQueryResult<UserEntity>> PageAsync(UserPageReqDto request)
    {
        var orderBy = OrderByConditionBuilder<UserEntity>.Build(OrderByType.Desc, entity => entity.CreateTime);
        orderBy.Next = OrderByConditionBuilder<UserEntity>.Build(OrderByType.Desc, entity => entity.Id);
        return await userRepository.PageQueryAsync(WhereExpressionUtil.Create<UserEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete)
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.UserName), entity => entity.UserName.Contains(request.UserName))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.PhoneNumber), entity => entity.PhoneNumber.Contains(request.PhoneNumber))
            .AndAlsoIF(!string.IsNullOrWhiteSpace(request.Email), entity => entity.Email.Contains(request.Email)), orderBy, request.PageNumber, request.PageSize);
    }

    public async Task<List<UserEntity>> GetAllUserAsync()
    {
        return (await userRepository.QueryAsync(WhereExpressionUtil.Create<UserEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete))).ToList();
    }

    public async Task<UserEntity> GetByIdAsync(long id)
    {
        return await userRepository.GetByIdAsync(id);
    }

    public async Task<UserEntity?> GetAsync(string username, string password)
    {
        return await userRepository.GetAsync(entity => entity.UserName == username && entity.Password == password && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId);
    }

    public async Task<bool> CheckPasswordAsync(long userId, string password)
    {
        var user = await userRepository.GetAsync(entity => entity.Id == userId && entity.Password == password);
        return user != null && user.Id > 0;
    }

    public async Task<bool> ChangePasswordAsync(long userId, string oldPassword, string newPassword)
    {
        if (!await CheckPasswordAsync(userId, oldPassword))
        {
            throw new ExplicitException("密码错误");
        }
        if (newPassword == oldPassword)
        {
            throw new ExplicitException("新密码不能和旧密码相同");
        }
        return await userRepository.UpdateAsync(new UserEntity { Password = newPassword }, entity => entity.Password, entity => entity.Id == userId) > 0;
    }

    public async Task<bool> ResetPasswordAsync(long userId, string? newPassword = null)
    {
        var updateUser = new UserEntity();
        if (string.IsNullOrEmpty(newPassword))
        {
            var paramEntity = await paramRepository.GetAsync(c => c.ParamKey == ConfigConst.UserInitPassword && c.State == CommonState.Enable);
            var userInitPassword = paramEntity?.ParamValue;
            if (!string.IsNullOrEmpty(userInitPassword))
            {
                var hash = new HashCryptoProvider();
                updateUser.Password = hash.MD5(userInitPassword).ToLower();
            }
        }
        else
        {
            var hash = new HashCryptoProvider();
            updateUser.Password = hash.MD5(updateUser.Password).ToLower();
        }
        return await userRepository.UpdateAsync(updateUser, entity => entity.Password, entity => entity.Id == userId) > 0;
    }
}