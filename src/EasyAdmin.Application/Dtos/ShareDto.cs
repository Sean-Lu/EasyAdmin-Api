using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 分享列表状态
/// </summary>
public enum ShareListStatus
{
    Normal = 0,
    Disabled = 1,
    Expired = 2,
    TargetDeleted = 3
}

/// <summary>
/// 分享列表查询
/// </summary>
public class ShareListReqDto : PageRequestBase
{
    /// <summary>
    /// 名称关键字
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 分享目标类型
    /// </summary>
    public ShareTargetType? TargetType { get; set; }

    /// <summary>
    /// 分享状态
    /// </summary>
    public ShareListStatus? Status { get; set; }
}

/// <summary>
/// 分享列表项
/// </summary>
public class ShareListItemDto
{
    /// <summary>
    /// 分享ID
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 目标类型
    /// </summary>
    public ShareTargetType TargetType { get; set; }
    /// <summary>
    /// 目标ID
    /// </summary>
    public long TargetId { get; set; }
    /// <summary>
    /// 目标名称
    /// </summary>
    public string TargetName { get; set; } = string.Empty;
    /// <summary>
    /// 分享码
    /// </summary>
    public string ShareCode { get; set; } = string.Empty;
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// 是否有密码
    /// </summary>
    public bool HasPassword { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    /// <summary>
    /// 分享状态
    /// </summary>
    public ShareListStatus Status { get; set; }
    /// <summary>
    /// 目标是否可用
    /// </summary>
    public bool TargetAvailable { get; set; }
}

/// <summary>
/// 分享目标请求
/// </summary>
public class ShareTargetRequestDto
{
    /// <summary>
    /// 目标类型
    /// </summary>
    public ShareTargetType TargetType { get; set; }
    /// <summary>
    /// 目标ID
    /// </summary>
    public long TargetId { get; set; }
}

/// <summary>
/// 分享保存请求
/// </summary>
public class ShareSaveDto : ShareTargetRequestDto
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    /// <summary>
    /// 分享密码
    /// </summary>
    public string? Password { get; set; }
}

/// <summary>
/// 分享启停请求
/// </summary>
public class ShareToggleDto : ShareTargetRequestDto
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 分享配置
/// </summary>
public class ShareConfigDto
{
    /// <summary>
    /// 是否存在
    /// </summary>
    public bool Exists { get; set; }
    /// <summary>
    /// 分享码
    /// </summary>
    public string? ShareCode { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    /// <summary>
    /// 是否有密码
    /// </summary>
    public bool HasPassword { get; set; }
    /// <summary>
    /// 分享密码
    /// </summary>
    public string? Password { get; set; }
}

/// <summary>
/// 公开分享状态
/// </summary>
public class PublicShareStatusDto
{
    /// <summary>
    /// 目标类型
    /// </summary>
    public ShareTargetType TargetType { get; set; }
    /// <summary>
    /// 需要密码
    /// </summary>
    public bool RequiresPassword { get; set; }
}

/// <summary>
/// 分享收藏目标
/// </summary>
public class ShareFavoriteTargetDto
{
    /// <summary>
    /// 分享ID
    /// </summary>
    public long ShareId { get; set; }
    /// <summary>
    /// 收藏目标类型
    /// </summary>
    public FavoriteTargetType TargetType { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// 分享者
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;
}

/// <summary>
/// 分享密码验证
/// </summary>
public class PublicShareVerifyDto
{
    /// <summary>
    /// 分享码
    /// </summary>
    public string ShareCode { get; set; } = string.Empty;
    /// <summary>
    /// 分享密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 分享访问令牌
/// </summary>
public class PublicShareVerifyResultDto
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    /// <summary>
    /// 有效分钟
    /// </summary>
    public int ExpireMinutes { get; set; }
}

/// <summary>
/// 公开文件信息
/// </summary>
public class PublicShareFileDto
{
    /// <summary>
    /// 分享者名称
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;
    /// <summary>
    /// 文件名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }
    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// 公开笔记内容
/// </summary>
public class PublicShareNoteDto
{
    /// <summary>
    /// 分享者名称
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// 正文格式
    /// </summary>
    public NoteContentType ContentType { get; set; }
    /// <summary>
    /// 正文HTML
    /// </summary>
    public string? ContentHtml { get; set; }
    /// <summary>
    /// 分类名称
    /// </summary>
    public string? CategoryName { get; set; }
    /// <summary>
    /// 标签名称
    /// </summary>
    public List<string> Tags { get; set; } = new();
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// 公开文件流
/// </summary>
public sealed record PublicShareStream(Stream Content, string? ContentType, string FileName);