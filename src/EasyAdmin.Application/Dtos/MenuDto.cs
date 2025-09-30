using AutoMapper;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

[AutoMap(typeof(MenuEntity), ReverseMap = true)]
public class MenuDto : TreeDtoBase<MenuDto>
{
    /// <summary>
    /// 图标
    /// </summary>
    public virtual string Icon { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    public virtual string Title { get; set; }
    /// <summary>
    /// 路由路径
    /// </summary>
    public virtual string Path { get; set; }
    /// <summary>
    /// 外部链接地址
    /// </summary>
    public virtual string OutLink { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 上级菜单完整路径
    /// </summary>
    public virtual string ParentFullPath { get; set; }
}