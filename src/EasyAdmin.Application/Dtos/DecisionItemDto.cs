using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 随机决策候选项DTO
/// </summary>
public class DecisionItemDto : TenantDtoBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 决策类型
    /// </summary>
    [Required]
    public DecisionItemType Type { get; set; }

    /// <summary>
    /// 候选项名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public CommonState State { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }
}

/// <summary>
/// 随机决策候选项更新DTO
/// </summary>
public class DecisionItemUpdateDto : DtoIdBase
{
    /// <summary>
    /// 决策类型
    /// </summary>
    [Required]
    public DecisionItemType Type { get; set; }

    /// <summary>
    /// 候选项名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public CommonState State { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }
}

/// <summary>
/// 随机抽取请求DTO
/// </summary>
public class DecisionDrawReqDto
{
    /// <summary>
    /// 决策类型
    /// </summary>
    [Required]
    public DecisionItemType Type { get; set; }

    /// <summary>
    /// 抽取数量
    /// </summary>
    public int Count { get; set; } = 1;
}
