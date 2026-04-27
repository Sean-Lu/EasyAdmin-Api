namespace EasyAdmin.Infrastructure.Enums;

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
    LocalFile = 0,
    /// <summary>
    /// 阿里云 OSS
    /// </summary>
    AliyunOSS = 1
}

/// <summary>
/// 定时任务调度类型
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// 简单调度
    /// </summary>
    Simple = 0,
    /// <summary>
    /// Cron调度
    /// </summary>
    Cron = 1
}

/// <summary>
/// 定时任务简单调度时间单位
/// </summary>
public enum SimpleIntervalUnit
{
    /// <summary>
    /// 秒
    /// </summary>
    Second = 0,
    /// <summary>
    /// 分钟
    /// </summary>
    Minute = 1,
    /// <summary>
    /// 小时
    /// </summary>
    Hour = 2,
    /// <summary>
    /// 天
    /// </summary>
    Day = 3
}