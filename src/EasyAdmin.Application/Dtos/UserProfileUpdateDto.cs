namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 当前用户资料更新 DTO
/// </summary>
public class UserProfileUpdateDto
{
    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 头像地址
    /// </summary>
    public long? AvatarFileId { get; set; }
}
