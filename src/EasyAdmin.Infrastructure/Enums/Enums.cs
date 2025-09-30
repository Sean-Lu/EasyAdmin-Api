namespace EasyAdmin.Infrastructure.Enums;

/// <summary>
/// 用户角色
/// </summary>
public enum UserRole
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// 普通用户
    /// </summary>
    User = 1,
    /// <summary>
    /// 管理员
    /// </summary>
    Administrator = 2,
    /// <summary>
    /// 超级管理员
    /// </summary>
    SuperAdministrator = 3
}

/// <summary>
/// 签到类型
/// </summary>
public enum CheckInType
{
    /// <summary>
    /// 上班打卡
    /// </summary>
    Working = 1
}

/// <summary>
/// 通用状态
/// </summary>
public enum CommonState
{
    /// <summary>
    /// 0-禁用
    /// </summary>
    Disable = 0,
    /// <summary>
    /// 1-启用
    /// </summary>
    Enable = 1
}

/// <summary>
/// 文件存储类型
/// </summary>
public enum FileStoreType
{
    /// <summary>
    /// 本地文件
    /// </summary>
    LocalFile = 0
}