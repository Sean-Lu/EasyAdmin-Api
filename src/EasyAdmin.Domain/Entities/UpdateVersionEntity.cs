using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 更新版本实体：记录每个已注册的客户端程序版本信息
/// - 同一 (AppCode, Platform) 组合同时只能有一个"已发布"版本（Status=Enable）
/// - VersionCode 是每个 (AppCode, Platform) 下独立的内部递增序列号
/// - MinSupportedVersionCode 定义最低兼容版本，低于此版本的客户端必须强制更新
/// </summary>
[CodeFirst]
public class UpdateVersionEntity : TenantEntityBase
{
    /// <summary>
    /// 客户端应用标识（如 "demo", "tool-a", "tool-b"），区分不同客户端程序
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string AppCode { get; set; }

    /// <summary>
    /// 版本号名称（如 v1.0.0）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string VersionName { get; set; }

    /// <summary>
    /// 内部版本码（同 AppCode+Platform 下递增，如 1,2,3...）
    /// </summary>
    [Required]
    public virtual int VersionCode { get; set; }

    /// <summary>
    /// 目标平台（win-x64 / win-x86 / linux-x64 / osx-x64）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Platform { get; set; }

    /// <summary>
    /// 版本更新日志 / Changelog，支持Markdown格式
    /// </summary>
    [MaxLength(5000)]
    public virtual string? Changelog { get; set; }

    /// <summary>
    /// 是否强制更新：开启后客户端不能跳过此版本
    /// </summary>
    public virtual bool IsForceUpdate { get; set; }

    /// <summary>
    /// 最低支持版本码：客户端版本低于此值 → 强制更新
    /// </summary>
    public virtual int MinSupportedVersionCode { get; set; }

    /// <summary>
    /// 版本发布状态：Enable=已发布(客户端可检测), Disable=未发布
    /// </summary>
    [DefaultValue(CommonState.Disable)]
    public virtual CommonState State { get; set; }

    /// <summary>
    /// 版本发布时间（首次设置为Enable时自动赋值）
    /// </summary>
    public virtual DateTime? PublishTime { get; set; }
}