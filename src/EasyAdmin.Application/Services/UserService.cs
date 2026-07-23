using System.Text.RegularExpressions;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Helper;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Microsoft.Extensions.Logging;
using Sean.Core.DbRepository.Util;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Utility.Security.Provider;
using EasyAdmin.Infrastructure.Const;
using MapsterMapper;

namespace EasyAdmin.Application.Services;

public class UserService(
    ILogger<UserService> logger,
    IMapper mapper,
    IUserRepository userRepository,
    IParamRepository paramRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository
    ) : IUserService
{
    public async Task<UserEntity> RegisterAsync(RegisterUserDto dto, long tenantId)
    {
        var userName = dto.UserName?.Trim();
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ExplicitException("用户名不能为空");
        }
        if (userName.Length > 50)
        {
            throw new ExplicitException("用户名不能超过50个字符");
        }
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length > 50)
        {
            throw new ExplicitException("密码不能为空且不能超过50个字符");
        }

        var phoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
        if (phoneNumber != null && !Regex.IsMatch(phoneNumber, @"^1\d{10}$"))
        {
            throw new ExplicitException("手机号码格式不正确");
        }

        var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        if (email != null && !Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        {
            throw new ExplicitException("邮箱格式不正确");
        }

        if (await userRepository.ExistsAsync(entity => entity.UserName == userName && !entity.IsDelete && entity.TenantId == tenantId, false))
        {
            throw new ExplicitException("用户名已存在");
        }
        if (phoneNumber != null && await userRepository.ExistsAsync(entity => entity.PhoneNumber == phoneNumber && !entity.IsDelete && entity.TenantId == tenantId, false))
        {
            throw new ExplicitException("手机号已存在");
        }
        if (email != null && await userRepository.ExistsAsync(entity => entity.Email == email && !entity.IsDelete && entity.TenantId == tenantId, false))
        {
            throw new ExplicitException("邮箱已存在");
        }

        var normalUserRole = await roleRepository.GetAsync(entity =>
            entity.TenantId == tenantId &&
            entity.Code == SysConst.NormalUserRoleCode &&
            entity.State == CommonState.Enable &&
            !entity.IsDelete);
        if (normalUserRole == null || normalUserRole.Id < 1)
        {
            throw new ExplicitException("普通用户角色不存在，请联系管理员");
        }

        var user = new UserEntity
        {
            TenantId = tenantId,
            UserName = userName,
            NickName = userName,
            Password = dto.Password.ToLower(),
            PhoneNumber = phoneNumber,
            Email = email,
            State = CommonState.Enable,
            ApprovalState = await IsRegistrationApprovalRequiredAsync() ? UserApprovalState.Pending : UserApprovalState.NotRequired
        };

        await userRepository.ExecuteAutoTransactionAsync(async transaction =>
        {
            if (!await userRepository.AddAsync(user, transaction: transaction))
            {
                throw new ExplicitException("注册用户失败");
            }

            if (!await userRoleRepository.AddAsync(new UserRoleEntity
            {
                TenantId = tenantId,
                UserId = user.Id,
                RoleId = normalUserRole.Id
            }, transaction: transaction))
            {
                throw new ExplicitException("分配普通用户角色失败");
            }

            return true;
        });

        return user;
    }

    private async Task<bool> IsRegistrationApprovalRequiredAsync()
    {
        var parameter = await paramRepository.GetAsync(entity =>
            entity.ParamKey == ConfigConst.UserRegisterNeedApproval &&
            entity.State == CommonState.Enable &&
            !entity.IsDelete);
        return bool.TryParse(parameter?.ParamValue, out var required) && required;
    }

    public async Task<bool> ApproveAsync(long id)
    {
        return await userRepository.UpdateAsync(
            new UserEntity { ApprovalState = UserApprovalState.Approved },
            entity => new { entity.ApprovalState },
            entity => entity.Id == id && entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete && entity.ApprovalState == UserApprovalState.Pending) > 0;
    }

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
        entity.ApprovalState = UserApprovalState.NotRequired;
        if (string.IsNullOrWhiteSpace(entity.NickName))
        {
            entity.NickName = entity.UserName;
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

    public async Task<bool> UpdateAsync(UserUpdateDto dto)
    {
        return await userRepository.UpdateByDtoAsync(dto, mapper.Map<UserEntity>) > 0;
    }
    public async Task<bool> UpdateProfileAsync(long userId, UserProfileUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NickName))
        {
            throw new ExplicitException("昵称不能为空");
        }

        var currentUser = await userRepository.GetAsync(entity => entity.Id == userId && entity.TenantId == TenantContextHolder.TenantId);
        if (currentUser == null || currentUser.Id < 1)
        {
            throw new ExplicitException("用户不存在");
        }

        var phoneNumberChanged = !string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber != currentUser.PhoneNumber;
        var emailChanged = !string.IsNullOrWhiteSpace(dto.Email) && dto.Email != currentUser.Email;

        if (phoneNumberChanged || emailChanged)
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || !await CheckPasswordAsync(userId, dto.CurrentPassword))
            {
                throw new ExplicitException("密码错误");
            }
        }

        if (phoneNumberChanged)
        {
            if (!Regex.IsMatch(dto.PhoneNumber!, @"^1\d{10}$"))
            {
                throw new ExplicitException("手机号格式不正确");
            }
            if (await userRepository.ExistsAsync(entity => entity.PhoneNumber == dto.PhoneNumber && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId && entity.Id != userId))
            {
                throw new ExplicitException("手机号已存在");
            }
        }

        if (emailChanged)
        {
            if (!Regex.IsMatch(dto.Email!, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                throw new ExplicitException("邮箱格式不正确");
            }
            if (await userRepository.ExistsAsync(entity => entity.Email == dto.Email && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId && entity.Id != userId))
            {
                throw new ExplicitException("邮箱已存在");
            }
        }

        return await userRepository.UpdateAsync(
            new UserEntity
            {
                NickName = dto.NickName.Trim(),
                AvatarFileId = dto.AvatarFileId,
                PhoneNumber = phoneNumberChanged ? dto.PhoneNumber : currentUser.PhoneNumber,
                Email = emailChanged ? dto.Email : currentUser.Email
            },
            entity => new { entity.NickName, entity.AvatarFileId, entity.PhoneNumber, entity.Email },
            entity => entity.Id == userId && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }
    public async Task<bool> UpdateStateAsync(long id, CommonState state)
    {
        return await userRepository.UpdateAsync(new UserEntity { State = state }, entity => new { entity.State }, entity => entity.Id == id && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }
    public async Task<bool> UpdateLastLoginTimeAsync(long id, long tenantId, DateTime lastLoginTime)
    {
        return await userRepository.UpdateAsync(new UserEntity { LastLoginTime = lastLoginTime }, entity => new { entity.LastLoginTime }, entity => entity.Id == id && entity.TenantId == tenantId) > 0;
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
        return (await userRepository.QueryAsync(WhereExpressionUtil.Create<UserEntity>(entity => entity.TenantId == TenantContextHolder.TenantId && !entity.IsDelete && entity.State == CommonState.Enable))).ToList();
    }

    public async Task<UserEntity> GetByIdAsync(long id)
    {
        return await userRepository.GetByIdAsync(id);
    }

    public async Task<UserEntity?> GetAsync(string username, string password)
    {
        return await userRepository.GetAsync(entity => entity.UserName == username && entity.Password == password && !entity.IsDelete && entity.TenantId == TenantContextHolder.TenantId);
    }

    public async Task<UserEntity?> GetByAccountAsync(string account, string password, LoginType loginType, long tenantId)
    {
        if (string.IsNullOrWhiteSpace(account))
        {
            return null;
        }

        var trimmedAccount = account.Trim();
        // 根据登录方式选择对应字段；Password 模式下根据账号格式自动识别为用户名/手机号/邮箱
        if (loginType == LoginType.PhoneCode || AccountFormatHelper.IsPhoneNumber(trimmedAccount))
        {
            return await userRepository.GetAsync(entity =>
                entity.PhoneNumber == trimmedAccount
                && entity.Password == password
                && !entity.IsDelete
                && entity.TenantId == tenantId);
        }
        if (loginType == LoginType.EmailCode || AccountFormatHelper.IsEmail(trimmedAccount))
        {
            return await userRepository.GetAsync(entity =>
                entity.Email == trimmedAccount
                && entity.Password == password
                && !entity.IsDelete
                && entity.TenantId == tenantId);
        }
        return await userRepository.GetAsync(entity =>
            entity.UserName == trimmedAccount
            && entity.Password == password
            && !entity.IsDelete
            && entity.TenantId == tenantId);
    }

    public async Task<bool> CheckPasswordAsync(long userId, string password)
    {
        var user = await userRepository.GetAsync(entity => entity.Id == userId && entity.Password == password && entity.TenantId == TenantContextHolder.TenantId);
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
        return await userRepository.UpdateAsync(new UserEntity { Password = newPassword }, entity => entity.Password, entity => entity.Id == userId && entity.TenantId == TenantContextHolder.TenantId) > 0;
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
        return await userRepository.UpdateAsync(updateUser, entity => entity.Password, entity => entity.Id == userId && entity.TenantId == TenantContextHolder.TenantId) > 0;
    }
}