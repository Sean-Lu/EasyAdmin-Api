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
/// 通知类型
/// </summary>
public enum NotificationNoticeType
{
    /// <summary>
    /// 普通
    /// </summary>
    Normal = 1,
    /// <summary>
    /// 重要
    /// </summary>
    Important = 2,
    /// <summary>
    /// 紧急
    /// </summary>
    Urgent = 3
}

/// <summary>
/// 随机决策类型
/// </summary>
public enum DecisionItemType
{
    /// <summary>
    /// 吃什么
    /// </summary>
    Food = 1,
    /// <summary>
    /// 去哪玩
    /// </summary>
    Place = 2
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
/// 文件业务分类
/// </summary>
public enum FileBizType
{
    /// <summary>
    /// 普通文件
    /// </summary>
    Normal = 0,
    /// <summary>
    /// 笔记图片
    /// </summary>
    NoteImage = 1,
    /// <summary>
    /// 用户头像
    /// </summary>
    UserAvatar = 2
}

/// <summary>
/// 分享目标类型
/// </summary>
public enum ShareTargetType
{
    /// <summary>
    /// 文件
    /// </summary>
    File = 0,
    /// <summary>
    /// 笔记
    /// </summary>
    Note = 1
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

/// <summary>
/// 登录方式
/// </summary>
public enum LoginType
{
    /// <summary>
    /// 账号密码登录（用户名/手机号/邮箱 + 密码）
    /// </summary>
    Password = 1,
    /// <summary>
    /// 手机短信验证码登录
    /// </summary>
    PhoneCode = 2,
    /// <summary>
    /// 邮箱验证码登录
    /// </summary>
    EmailCode = 3
}

/// <summary>
/// 笔记正文格式
/// </summary>
public enum NoteContentType
{
    /// <summary>
    /// 富文本
    /// </summary>
    RichText = 0,
    /// <summary>
    /// Markdown
    /// </summary>
    Markdown = 1
}