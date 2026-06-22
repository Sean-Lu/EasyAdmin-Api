using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 股票账户表
/// </summary>
[CodeFirst]
public class StockAccountEntity : TenantEntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }

    /// <summary>
    /// 券商名称
    /// </summary>
    [MaxLength(100)]
    public virtual string BrokerName { get; set; } = string.Empty;

    /// <summary>
    /// 初始资产
    /// </summary>
    public virtual decimal InitialAsset { get; set; }

    /// <summary>
    /// 现资产
    /// </summary>
    public virtual decimal CurrentAsset { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public virtual string? Remark { get; set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    [DefaultValue(0)]
    public virtual int SortOrder { get; set; }
}
