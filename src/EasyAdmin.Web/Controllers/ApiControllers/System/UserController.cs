using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Storage;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Filter;
using EasyAdmin.Web.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 用户管理
/// </summary>
public class UserController(
    ILogger<UserController> logger,
    IConfiguration configuration,
    IMapper mapper,
    IUserService userService,
    IUserRoleService userRoleService,
    IFileService fileService,
    IFileStorageFactory fileStorageFactory,
    IAccountAccessService accountAccessService,
    ITenantService tenantService
    ) : BaseApiController
{
    private const long AvatarMaxSize = 2 * 1024 * 1024;
    private static readonly HashSet<string> AvatarContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp"
    };
    private static readonly HashSet<string> AvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiRepeatRequestFilter]
    public async Task<ApiResult<bool>> Add(UserDto data)
    {
        if (string.IsNullOrEmpty(data.UserName))
        {
            return Fail<bool>("用户名称不能为空");
        }

        return Success(await userService.AddAsync(data));
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Delete([FromBody] JObject? data)
    {
        var ids = data["ids"]?.Values<long>().ToList() ?? default;
        if (ids != null && ids.Any())
        {
            // 批量删除
            return Success(await userService.DeleteByIdsAsync(ids));
        }

        // 单个删除
        var id = data["id"]?.Value<long>() ?? default;
        return Success(await userService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改用户
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> Update(UserUpdateDto data)
    {
        return Success(await userService.UpdateAsync(data));
    }

    /// <summary>
    /// 修改用户状态
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateState([FromBody] JObject? data)
    {
        var id = data["id"]?.Value<long>() ?? default;
        var state = (CommonState)(data["state"]?.Value<int>() ?? default);
        var result = await userService.UpdateStateAsync(id, state);
        if (result)
        {
            await accountAccessService.InvalidateUserAsync(TenantId, id);
        }
        return Success(result);
    }

    /// <summary>
    /// 审核通过用户
    /// </summary>
    [HttpPost]
    public async Task<ApiResult<bool>> Approve([FromBody] JObject? data)
    {
        var id = data?["id"]?.Value<long>() ?? default;
        var result = await userService.ApproveAsync(id);
        if (result)
        {
            await accountAccessService.InvalidateUserAsync(TenantId, id);
        }
        return Success(result);
    }

    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<ApiResultPageData<UserDto>>> Page([FromQuery] UserPageReqDto request)
    {
        var pageResult = await userService.PageAsync(request);
        return Success(mapper.Map<ApiResultPageData<UserDto>>(pageResult));
    }

    /// <summary>
    /// 查询用户列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<UserDto>>> List()
    {
        var pageResult = await userService.GetAllUserAsync();
        return Success(mapper.Map<List<UserDto>>(pageResult));
    }

    /// <summary>
    /// 查询用户详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<UserDto>> Detail(long id)
    {
        return Success(mapper.Map<UserDto>(await userService.GetByIdAsync(id)));
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<UserDto>> GetUserInfo()
    {
        var dto = mapper.Map<UserDto>(await userService.GetByIdAsync(UserId));
        var tenant = await tenantService.GetByIdAsync(TenantId);
        dto.TenantCode = tenant?.Code;
        dto.TenantName = tenant?.Name;
        return Success(dto);
    }

    /// <summary>
    /// 修改当前用户资料
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> UpdateProfile(UserProfileUpdateDto data)
    {
        var oldAvatarFileId = (await userService.GetByIdAsync(UserId))?.AvatarFileId;
        var result = await userService.UpdateProfileAsync(UserId, data);
        if (result && oldAvatarFileId != data.AvatarFileId && oldAvatarFileId is > 0)
        {
            var fileEntity = await fileService.GetByIdAsync(oldAvatarFileId.Value);
            if (fileEntity != null && fileEntity.Id >= 1)
            {
                await fileStorageFactory.GetFileStorage(fileEntity.StoreType).DeleteAsync(fileEntity.Path);
                await fileService.DeleteByIdAsync(oldAvatarFileId.Value);
            }
        }
        return Success(result);
    }

    /// <summary>
    /// 上传当前用户头像
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<long>> UploadAvatar(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return Fail<long>("头像不能为空");
        }
        if (file.Length > AvatarMaxSize)
        {
            return Fail<long>("头像大小不能超过 2MB");
        }
        if (!AvatarContentTypes.Contains(file.ContentType))
        {
            return Fail<long>("仅支持 JPG、PNG、GIF、WEBP 图片");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AvatarExtensions.Contains(extension))
        {
            return Fail<long>("仅支持 JPG、PNG、GIF、WEBP 图片");
        }

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var relativePath = $"UploadFiles/avatars/{TenantId}/{UserId}/{fileName}";
        await using var stream = file.OpenReadStream();
        var storeType = configuration.GetValue<FileStoreType>("FileStorage:StorageType");
        var filePath = await fileStorageFactory.GetFileStorage(storeType).UploadAsync(stream, relativePath);

        var fileDto = new FileDto
        {
            Name = file.FileName,
            Path = filePath,
            Size = file.Length,
            ContentType = file.ContentType,
            StoreType = storeType,
            BizType = FileBizType.UserAvatar,
            Description = "用户头像"
        };
        var fileId = await fileService.AddAndReturnIdAsync(fileDto);
        if (fileId < 1)
        {
            return Fail<long>("保存头像文件失败");
        }
        return Success(fileId);
    }

    /// <summary>
    /// 删除当前用户头像文件
    /// </summary>
    /// <param name="id">头像文件ID</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ApiResult<bool>> DeleteAvatarFile(long id)
    {
        var fileEntity = await fileService.GetByIdAsync(id);
        if (fileEntity == null || fileEntity.Id < 1)
        {
            return Success(true);
        }
        if (fileEntity.BizType != FileBizType.UserAvatar || fileEntity.TenantId != TenantId || fileEntity.CreateUserId != UserId)
        {
            return Fail<bool>("只能删除当前用户的头像文件");
        }

        try
        {
            await fileStorageFactory.GetFileStorage(fileEntity.StoreType).DeleteAsync(fileEntity.Path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "删除头像文件失败");
        }
        return Success(await fileService.DeleteByIdAsync(id));
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> ChangePassword([FromBody] JObject data)
    {
        var oldPassword = data["oldPassword"]?.Value<string>();
        var newPassword = data["newPassword"]?.Value<string>();
        if (!await userService.ChangePasswordAsync(UserId, oldPassword, newPassword))
        {
            return Fail<bool>("修改密码失败");
        }
        return Success(true);
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> ResetPassword([FromBody] JObject data)
    {
        //var ids = data["ids"]?.Values<long>().ToList() ?? default;
        var id = data["id"]?.Value<long>() ?? default;
        if (!await userService.ResetPasswordAsync(id))
        {
            return Fail<bool>("重置密码失败");
        }
        return Success(true);
    }

    /// <summary>
    /// 给用户分配角色
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<bool>> AssignRoles(UserRoleAssignmentDto data)
    {
        return Success(await userRoleService.AssignRolesToUserAsync(data));
    }

    /// <summary>
    /// 获取用户角色ID列表
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult<List<long>>> GetUserRoleIds(long userId)
    {
        return Success(await userRoleService.GetUserRoleIdsAsync(userId));
    }
}
