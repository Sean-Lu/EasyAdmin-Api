namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 笔记保护密码状态DTO
/// </summary>
public class NotePasswordStatusDto
{
    /// <summary>
    /// 是否已设置
    /// </summary>
    public bool HasPassword { get; set; }
}

/// <summary>
/// 笔记保护密码设置DTO
/// </summary>
public class NotePasswordSetDto
{
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
}

/// <summary>
/// 笔记保护密码修改DTO
/// </summary>
public class NotePasswordChangeDto
{
    /// <summary>
    /// 旧密码
    /// </summary>
    public string OldPassword { get; set; }
    /// <summary>
    /// 新密码
    /// </summary>
    public string NewPassword { get; set; }
}

/// <summary>
/// 笔记保护密码校验DTO
/// </summary>
public class NotePasswordVerifyDto
{
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
}

/// <summary>
/// 笔记保护密码校验结果DTO
/// </summary>
public class NotePasswordVerifyResultDto
{
    /// <summary>
    /// 解锁令牌
    /// </summary>
    public string UnlockToken { get; set; }
    /// <summary>
    /// 过期分钟
    /// </summary>
    public int ExpireMinutes { get; set; }
}
