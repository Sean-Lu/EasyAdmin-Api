using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Domain.Contracts;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 数据库表实体基类
/// </summary>
[NamingConvention(NamingConvention.PascalCase)]
public abstract class EntityBase : IEntityBase
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column(Order = 1)]
    [Description("主键")]
    public virtual long Id { get; set; }

    [Description("创建人ID")]
    public virtual long CreateUserId { get; set; }
    [Description("创建时间")]
    public virtual DateTime? CreateTime { get; set; }
    [Description("更新人ID")]
    public virtual long UpdateUserId { get; set; }
    [Description("更新时间")]
    public virtual DateTime? UpdateTime { get; set; }

    [Description("是否删除")]
    public virtual bool IsDelete { get; set; }
}

/// <summary>
/// 数据库表树结构实体基类
/// </summary>
public abstract class TreeEntityBase<TEntity> : EntityBase, ITreeEntityBase<TEntity>
{
    [Required]
    [Column(Order = 2)]
    [Description("父主键")]
    public virtual long PId { get; set; }
    [Description("排序")]
    public int Sort { get; set; }

    [NotMapped]
    public virtual List<TEntity>? Children { get; set; }
}

/// <summary>
/// 数据库表租户实体基类
/// </summary>
public abstract class TenantEntityBase : EntityBase, ITenantEntityBase
{
    [Description("租户ID")]
    public virtual long TenantId { get; set; }
}

/// <summary>
/// 数据库表租户树结构实体基类
/// </summary>
public abstract class TenantTreeEntityBase<TEntity> : TenantEntityBase, ITenantTreeEntityBase<TEntity>
{
    [Required]
    [Column(Order = 2)]
    [Description("父主键")]
    public virtual long PId { get; set; }
    [Description("排序")]
    public int Sort { get; set; }

    [NotMapped]
    public List<TEntity>? Children { get; set; }
}