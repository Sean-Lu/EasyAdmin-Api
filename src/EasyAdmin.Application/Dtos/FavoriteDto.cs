using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 收藏目标请求
/// </summary>
public class FavoriteTargetReqDto
{
    /// <summary>
    /// 目标类型
    /// </summary>
    public FavoriteTargetType TargetType { get; set; }
    /// <summary>
    /// 目标ID
    /// </summary>
    public long TargetId { get; set; }
}

/// <summary>
/// 分享收藏请求
/// </summary>
public class FavoriteAddShareReqDto
{
    /// <summary>
    /// 分享码
    /// </summary>
    public string ShareCode { get; set; } = string.Empty;
    /// <summary>
    /// 分享访问令牌
    /// </summary>
    public string? AccessToken { get; set; }
}

/// <summary>
/// 取消收藏请求
/// </summary>
public class FavoriteDeleteReqDto
{
    /// <summary>
    /// 收藏ID
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 收藏状态请求
/// </summary>
public class FavoriteStatusReqDto
{
    /// <summary>
    /// 直接目标
    /// </summary>
    public List<FavoriteTargetReqDto> Targets { get; set; } = new();
    /// <summary>
    /// 分享码
    /// </summary>
    public string? ShareCode { get; set; }
}

/// <summary>
/// 收藏状态项
/// </summary>
public class FavoriteStatusItemDto
{
    /// <summary>
    /// 目标类型
    /// </summary>
    public FavoriteTargetType TargetType { get; set; }
    /// <summary>
    /// 来源类型
    /// </summary>
    public FavoriteSourceType SourceType { get; set; }
    /// <summary>
    /// 目标ID
    /// </summary>
    public long TargetId { get; set; }
    /// <summary>
    /// 分享码
    /// </summary>
    public string? ShareCode { get; set; }
    /// <summary>
    /// 是否已收藏
    /// </summary>
    public bool IsFavorite { get; set; }
    /// <summary>
    /// 收藏ID
    /// </summary>
    public long? FavoriteId { get; set; }
}

/// <summary>
/// 收藏变更结果
/// </summary>
public class FavoriteMutationResultDto
{
    /// <summary>
    /// 是否已收藏
    /// </summary>
    public bool IsFavorite { get; set; }
    /// <summary>
    /// 收藏ID
    /// </summary>
    public long? FavoriteId { get; set; }
}

/// <summary>
/// 收藏分页请求
/// </summary>
public class FavoritePageReqDto : PageRequestBase
{
    /// <summary>
    /// 目标类型
    /// </summary>
    public FavoriteTargetType TargetType { get; set; }
    /// <summary>
    /// 关键词
    /// </summary>
    public string? Keyword { get; set; }
    /// <summary>
    /// 可用状态
    /// </summary>
    public FavoriteAvailabilityStatus? Status { get; set; }
}

/// <summary>
/// 收藏列表项
/// </summary>
public class FavoriteListItemDto
{
    /// <summary>
    /// 收藏ID
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 目标类型
    /// </summary>
    public FavoriteTargetType TargetType { get; set; }
    /// <summary>
    /// 来源类型
    /// </summary>
    public FavoriteSourceType SourceType { get; set; }
    /// <summary>
    /// 直接目标ID
    /// </summary>
    public long? DirectTargetId { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// 分享者
    /// </summary>
    public string? OwnerName { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }
    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType { get; set; }
    /// <summary>
    /// 路由路径
    /// </summary>
    public string? Path { get; set; }
    /// <summary>
    /// 外部链接
    /// </summary>
    public string? OutLink { get; set; }
    /// <summary>
    /// 外链打开方式
    /// </summary>
    public OutLinkOpenType? OutLinkOpenType { get; set; }
    /// <summary>
    /// 分享码
    /// </summary>
    public string? ShareCode { get; set; }
    /// <summary>
    /// 可用状态
    /// </summary>
    public FavoriteAvailabilityStatus Status { get; set; }
    /// <summary>
    /// 是否可用
    /// </summary>
    public bool IsAvailable { get; set; }
    /// <summary>
    /// 收藏时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
}
