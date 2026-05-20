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

/// <summary>
/// 外链打开方式
/// </summary>
public enum OutLinkOpenType
{
    /// <summary>
    /// 在当前Tab页面内嵌打开（iframe方式）
    /// </summary>
    Inline = 0,
    /// <summary>
    /// 在新标签页打开（target="_blank"）
    /// </summary>
    Blank = 1
}

/// <summary>
/// 代码生成数据库类型
/// </summary>
public enum CodeGenDbType
{
    /// <summary>
    /// MySQL
    /// </summary>
    MySql = 0,
    /// <summary>
    /// SQL Server
    /// </summary>
    SqlServer = 1,
    /// <summary>
    /// PostgreSQL
    /// </summary>
    PostgreSql = 2
}

/// <summary>
/// 代码生成模板类型
/// </summary>
public enum CodeGenTemplateType
{
    /// <summary>
    /// 内置模板
    /// </summary>
    BuiltIn = 0,
    /// <summary>
    /// 用户上传
    /// </summary>
    UserUpload = 1
}


/// <summary>
/// 更新包类型
/// </summary>
public enum UpdatePackageType
{
    /// <summary>
    /// 全量更新包
    /// </summary>
    Full = 0,
    /// <summary>
    /// 增量更新包
    /// </summary>
    Incremental = 1
}

/// <summary>
/// Token模式
/// </summary>
public enum TokenMode
{
    /// <summary>
    /// 单token模式
    /// </summary>
    Single,
    /// <summary>
    /// 双token模式（AccessToken、RefreshToken）
    /// </summary>
    Refresh
}