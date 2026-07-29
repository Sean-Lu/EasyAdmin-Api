using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Application.Dtos;

/// <summary>
/// AI草稿
/// </summary>
public sealed class AiDraftDto
{
    /// <summary>
    /// 草稿ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public long ConversationId { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// 草稿类型
    /// </summary>
    public AiDraftType DraftType { get; set; }

    /// <summary>
    /// 草稿内容
    /// </summary>
    public string ContentJson { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 确认开始时间
    /// </summary>
    public DateTime? ConfirmationStartedAt { get; set; }

    /// <summary>
    /// 已创建业务数据ID
    /// </summary>
    public long? ConfirmedTargetId { get; set; }

    /// <summary>
    /// 草稿状态
    /// </summary>
    public AiDraftStatus Status { get; set; }
}

/// <summary>
/// AI草稿更新参数
/// </summary>
public sealed class AiDraftUpdateDto
{
    /// <summary>
    /// 草稿ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 草稿内容
    /// </summary>
    public string ContentJson { get; set; } = string.Empty;
}

/// <summary>
/// AI草稿确认结果
/// </summary>
public sealed class AiDraftConfirmResultDto
{
    /// <summary>
    /// 草稿ID
    /// </summary>
    public long DraftId { get; set; }

    /// <summary>
    /// 草稿类型
    /// </summary>
    public AiDraftType DraftType { get; set; }

    /// <summary>
    /// 已创建业务数据ID
    /// </summary>
    public long ConfirmedTargetId { get; set; }
}

/// <summary>
/// 笔记草稿内容
/// </summary>
/// <param name="Title">标题</param>
/// <param name="ContentMarkdown">Markdown内容</param>
public sealed record AiNoteDraft(string Title, string ContentMarkdown);

/// <summary>
/// 待办草稿内容
/// </summary>
/// <param name="Name">名称</param>
/// <param name="CategoryId">分类ID</param>
/// <param name="Priority">优先级</param>
public sealed record AiTodoDraft(string Name, long CategoryId, int Priority);

/// <summary>
/// 日报草稿内容
/// </summary>
/// <param name="RecordTime">记录时间</param>
/// <param name="TodayWork">当日内容</param>
/// <param name="TomorrowPlan">次日计划</param>
public sealed record AiDayReportDraft(DateTime RecordTime, string TodayWork, string? TomorrowPlan);

/// <summary>
/// 周报草稿内容
/// </summary>
/// <param name="StartTime">开始时间</param>
/// <param name="EndTime">结束时间</param>
/// <param name="WeekWork">本周内容</param>
/// <param name="NextWeekPlan">下周计划</param>
public sealed record AiWeekReportDraft(
    DateTime StartTime,
    DateTime EndTime,
    string WeekWork,
    string? NextWeekPlan);

/// <summary>
/// 月报草稿内容
/// </summary>
/// <param name="StartTime">开始时间</param>
/// <param name="EndTime">结束时间</param>
/// <param name="MonthWork">本月内容</param>
/// <param name="NextMonthPlan">下月计划</param>
public sealed record AiMonthReportDraft(
    DateTime StartTime,
    DateTime EndTime,
    string MonthWork,
    string? NextMonthPlan);