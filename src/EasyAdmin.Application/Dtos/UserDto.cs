using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 用户 DTO
/// </summary>
public class UserDto : TenantDtoBase
{
    /// <summary>
    /// 租户编码
    /// </summary>
    public virtual string? TenantCode { get; set; }
    /// <summary>
    /// 租户名称
    /// </summary>
    public virtual string? TenantName { get; set; }
    /// <summary>
    /// 用户名称
    /// </summary>
    public virtual string UserName { get; set; }
    /// <summary>
    /// 昵称
    /// </summary>
    public virtual string? NickName { get; set; }
    /// <summary>
    /// 头像地址
    /// </summary>
    public virtual long? AvatarFileId { get; set; }
    /// <summary>
    /// 手机号码
    /// </summary>
    public virtual string? PhoneNumber { get; set; }
    /// <summary>
    /// 邮箱地址
    /// </summary>
    public virtual string? Email { get; set; }
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public virtual DateTime? LastLoginTime { get; set; }
    /// <summary>
    /// 所属部门ID
    /// </summary>
    public virtual long? DepartmentId { get; set; }
    /// <summary>
    /// 所属部门名称
    /// </summary>
    public virtual string? DepartmentName { get; set; }
    /// <summary>
    /// 所属岗位ID
    /// </summary>
    public virtual long? PositionId { get; set; }
    /// <summary>
    /// 所属岗位名称
    /// </summary>
    public virtual string? PositionName { get; set; }
    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    public virtual CommonState State { get; set; }
    /// <summary>
    /// 审核状态
    /// </summary>
    public virtual UserApprovalState ApprovalState { get; set; }
}