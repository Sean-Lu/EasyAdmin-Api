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

    /// <summary>
    /// 手机号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 当前登录密码（修改手机号或邮箱时必填，MD5）
    /// </summary>
    public string? CurrentPassword { get; set; }
}
