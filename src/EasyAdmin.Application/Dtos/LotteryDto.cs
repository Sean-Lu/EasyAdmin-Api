using System.ComponentModel.DataAnnotations;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// 抽奖活动DTO
/// </summary>
public class LotteryActivityDto : TenantDtoBase
{
    public long UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool AllowRepeatWinner { get; set; }

    public CommonState State { get; set; }
}

/// <summary>
/// 抽奖活动更新DTO
/// </summary>
public class LotteryActivityUpdateDto : DtoIdBase
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool AllowRepeatWinner { get; set; }

    public CommonState State { get; set; }
}

/// <summary>
/// 抽奖奖项DTO
/// </summary>
public class LotteryPrizeDto : TenantDtoBase
{
    [Required]
    public long ActivityId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public int Quota { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public CommonState State { get; set; }

    public int Sort { get; set; }

    public int WinnerCount { get; set; }
}

/// <summary>
/// 抽奖奖项更新DTO
/// </summary>
public class LotteryPrizeUpdateDto : DtoIdBase
{
    [Required]
    public long ActivityId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public int Quota { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public CommonState State { get; set; }

    public int Sort { get; set; }
}

/// <summary>
/// 抽奖参与人DTO
/// </summary>
public class LotteryParticipantDto : TenantDtoBase
{
    [Required]
    public long ActivityId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(100)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public CommonState State { get; set; }

    public int Sort { get; set; }
}

/// <summary>
/// 抽奖参与人更新DTO
/// </summary>
public class LotteryParticipantUpdateDto : DtoIdBase
{
    [Required]
    public long ActivityId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(100)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public CommonState State { get; set; }

    public int Sort { get; set; }
}

/// <summary>
/// 抽奖参与人批量导入DTO
/// </summary>
public class LotteryParticipantImportReqDto
{
    [Required]
    public long ActivityId { get; set; }

    [Required]
    public string Content { get; set; }
}

/// <summary>
/// 抽奖中奖记录DTO
/// </summary>
public class LotteryWinnerDto : TenantDtoBase
{
    public long ActivityId { get; set; }

    public long PrizeId { get; set; }

    public long ParticipantId { get; set; }

    public string BatchNo { get; set; }

    public string WinnerNameSnapshot { get; set; }

    public string PrizeNameSnapshot { get; set; }
}

/// <summary>
/// 抽奖请求DTO
/// </summary>
public class LotteryDrawReqDto
{
    [Required]
    public long ActivityId { get; set; }

    [Required]
    public long PrizeId { get; set; }

    public int Count { get; set; } = 1;
}

/// <summary>
/// 抽奖活动详情DTO
/// </summary>
public class LotteryActivityDetailDto
{
    public LotteryActivityDto? Activity { get; set; }

    public List<LotteryPrizeDto> Prizes { get; set; } = new();

    public List<LotteryParticipantDto> Participants { get; set; } = new();

    public List<LotteryWinnerDto> Winners { get; set; } = new();
}
