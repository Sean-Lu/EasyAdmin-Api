using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;
using System.ComponentModel.DataAnnotations.Schema;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 菜单表
/// </summary>
//[Table("Menu")]
[CodeFirst]
public class MenuEntity : TreeEntityBase<MenuEntity>
{
    /// <summary>
    /// 图标
    /// </summary>
    [MaxLength(50)]
    public virtual string Icon { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    [MaxLength(50)]
    public virtual string Title { get; set; }
    /// <summary>
    /// 路由路径
    /// </summary>
    [MaxLength(200)]
    public virtual string Path { get; set; }
    /// <summary>
    /// 外部链接地址
    /// </summary>
    [MaxLength(200)]
    public virtual string OutLink { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 上级菜单完整路径
    /// </summary>
    [NotMapped]
    public virtual string ParentFullPath { get; set; }
}