using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 租户 DTO
/// </summary>
public class TenantDto : DtoBase
{
    /// <summary>
    /// 租户编码
    /// </summary>
    public virtual string Code { get; set; }
    /// <summary>
    /// 租户名称
    /// </summary>
    public virtual string Name { get; set; }
    /// <summary>
    /// 租管账号ID
    /// </summary>
    public virtual long AdminUserId { get; set; }
    /// <summary>
    /// 生效时间
    /// </summary>
    public virtual DateTime? StartTime { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    public virtual DateTime? ExpireTime { get; set; }
    /// <summary>
    /// 租管账号名称
    /// </summary>
    public virtual string AdminUserName { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Remark { get; set; }
}