using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;
using Sean.Core.DbRepository;

namespace EasyAdmin.Domain.Entities;

/// <summary>
/// 应用标识表
/// </summary>
[CodeFirst]
public class AppCodeEntity : EntityBase
{
    /// <summary>
    /// 应用标识（如 "demo", "tool-a", "tool-b"）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 应用名称（如 "Demo App", "Tool A", "Tool B"）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 应用描述（如 "这是一个演示应用"）
    /// </summary>
    [MaxLength(500)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// 状态（0-禁用，1-启用）
    /// </summary>
    [DefaultValue(CommonState.Enable)]
    public virtual CommonState State { get; set; }
}