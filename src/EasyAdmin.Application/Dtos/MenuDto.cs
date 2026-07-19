using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 菜单 DTO
/// </summary>
public class MenuDto : TreeDtoBase<MenuDto>
{
    /// <summary>
    /// 所属租户ID，空值表示全局菜单
    /// </summary>
    public virtual long? TenantId { get; set; }
    /// <summary>
    /// 菜单类型
    /// </summary>
    public virtual MenuType Type { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public virtual string? Icon { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    public virtual string Title { get; set; }
    /// <summary>
    /// 路由路径
    /// </summary>
    public virtual string? Path { get; set; }
    /// <summary>
    /// 外部链接地址
    /// </summary>
    public virtual string? OutLink { get; set; }
    /// <summary>
    /// 外链打开方式（0-内嵌打开，1-新标签页打开）
    /// </summary>
    public virtual OutLinkOpenType? OutLinkOpenType { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 上级菜单完整路径
    /// </summary>
    public virtual string? ParentFullPath { get; set; }
}