using System.Text.Json;
using System.Text.Json.Serialization;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Application.Services;

/// <summary>
/// AI草稿服务实现
/// </summary>
public sealed class AiDraftService(
    IAiDraftRepository draftRepository,
    IAiConversationService conversationService,
    INoteService noteService,
    ITodoItemService todoItemService,
    IDayWorkReportService dayWorkReportService,
    IWeekWorkReportService weekWorkReportService,
    IMonthWorkReportService monthWorkReportService) : IAiDraftService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    /// <inheritdoc />
    public async Task<AiDraftDto> GetAsync(long id)
    {
        return ToDto(await GetOwnedAsync(id));
    }

    /// <inheritdoc />
    public async Task<AiDraftDto> UpdateAsync(AiDraftUpdateDto request)
    {
        var draft = await GetOwnedAsync(request.Id);
        EnsurePending(draft);
        draft.ContentJson = NormalizeAndValidate(draft.DraftType, request.ContentJson);
        var affected = await draftRepository.UpdateAsync(
            draft,
            item => item.ContentJson,
            OwnedPendingPredicate(draft.Id));
        if (affected != 1)
        {
            throw new ExplicitException("草稿状态已变化，请刷新后重试");
        }
        return ToDto(draft);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(long id)
    {
        var draft = await GetOwnedAsync(id);
        EnsurePending(draft);
        var affected = await draftRepository.UpdateAsync(
            new AiDraftEntity { IsDelete = true, Status = AiDraftStatus.Deleted },
            item => new { item.IsDelete, item.Status },
            OwnedPendingPredicate(id));
        if (affected != 1)
        {
            throw new ExplicitException("草稿状态已变化，请刷新后重试");
        }
    }

    /// <inheritdoc />
    public async Task<AiDraftConfirmResultDto> ConfirmAsync(long id)
    {
        var draft = await GetOwnedAsync(id, includeDeleted: true);
        if (draft.Status == AiDraftStatus.Confirmed && draft.ConfirmedTargetId.HasValue)
        {
            return ToConfirmResult(draft);
        }
        if (draft.Status == AiDraftStatus.Confirming)
        {
            throw new ExplicitException("草稿确认状态待核查，请联系管理员");
        }
        EnsurePending(draft);
        draft.ContentJson = NormalizeAndValidate(draft.DraftType, draft.ContentJson);
        var now = DateTime.Now;
        var locked = await draftRepository.UpdateAsync(
            new AiDraftEntity
            {
                Status = AiDraftStatus.Confirming,
                ConfirmationStartedAt = now
            },
            item => new { item.Status, item.ConfirmationStartedAt },
            item =>
                item.Id == id &&
                item.TenantId == TenantContextHolder.TenantId &&
                item.UserId == TenantContextHolder.UserId &&
                item.Status == AiDraftStatus.Pending &&
                !item.IsDelete &&
                item.ExpiresAt > now);
        if (locked != 1)
        {
            var current = await GetOwnedAsync(id, includeDeleted: true);
            if (current.Status == AiDraftStatus.Confirmed && current.ConfirmedTargetId.HasValue)
            {
                return ToConfirmResult(current);
            }
            if (current.Status == AiDraftStatus.Confirming)
            {
                throw new ExplicitException("草稿确认状态待核查，请联系管理员");
            }
            throw new ExplicitException("草稿状态已变化，请刷新后重试");
        }

        long targetId;
        try
        {
            targetId = await CreateTargetAsync(draft);
        }
        catch (TargetCreationUncertainException)
        {
            throw new ExplicitException("草稿确认状态待核查，请联系管理员");
        }
        catch
        {
            await draftRepository.UpdateAsync(
                new AiDraftEntity
                {
                    Status = AiDraftStatus.Pending,
                    ConfirmationStartedAt = null
                },
                item => new { item.Status, item.ConfirmationStartedAt },
                item =>
                    item.Id == id &&
                    item.TenantId == TenantContextHolder.TenantId &&
                    item.UserId == TenantContextHolder.UserId &&
                    item.Status == AiDraftStatus.Confirming &&
                    !item.IsDelete);
            throw;
        }

        var completed = await draftRepository.UpdateAsync(
            new AiDraftEntity
            {
                Status = AiDraftStatus.Confirmed,
                ConfirmedTargetId = targetId
            },
            item => new { item.Status, item.ConfirmedTargetId },
            item =>
                item.Id == id &&
                item.TenantId == TenantContextHolder.TenantId &&
                item.UserId == TenantContextHolder.UserId &&
                item.Status == AiDraftStatus.Confirming &&
                !item.IsDelete);
        if (completed != 1)
        {
            throw new ExplicitException("草稿确认状态待核查，请联系管理员");
        }
        draft.Status = AiDraftStatus.Confirmed;
        draft.ConfirmedTargetId = targetId;
        return ToConfirmResult(draft);
    }

    /// <inheritdoc />
    public async Task<AiDraftEntity> CreateFromModelAsync(
        long conversationId,
        long messageId,
        AiDraftType type,
        string contentJson)
    {
        await conversationService.GetOwnedAsync(conversationId);
        var entity = new AiDraftEntity
        {
            TenantId = TenantContextHolder.TenantId,
            UserId = TenantContextHolder.UserId,
            ConversationId = conversationId,
            MessageId = messageId,
            DraftType = type,
            ContentJson = NormalizeAndValidate(type, contentJson),
            ExpiresAt = DateTime.Now.AddHours(1),
            Status = AiDraftStatus.Pending
        };
        if (!await draftRepository.AddAsync(entity))
        {
            throw new ExplicitException("草稿创建失败");
        }
        return entity;
    }

    private async Task<long> CreateTargetAsync(AiDraftEntity draft)
    {
        var userId = TenantContextHolder.UserId;
        var success = false;
        var targetId = 0L;
        try
        {
            switch (draft.DraftType)
            {
                case AiDraftType.Note:
                    var note = Deserialize<AiNoteDraft>(draft.ContentJson);
                    var noteDto = new NoteUpdateDto
                    {
                        Title = note.Title.Trim(),
                        ContentType = NoteContentType.Markdown,
                        ContentMarkdown = note.ContentMarkdown,
                        Tags = []
                    };
                    try
                    {
                        success = await noteService.AddAsync(noteDto);
                    }
                    finally
                    {
                        targetId = noteDto.Id;
                    }
                    break;
                case AiDraftType.Todo:
                    var todo = Deserialize<AiTodoDraft>(draft.ContentJson);
                    var todoDto = new TodoItemDto
                    {
                        CategoryId = todo.CategoryId,
                        Name = todo.Name.Trim(),
                        Priority = todo.Priority
                    };
                    try
                    {
                        success = await todoItemService.AddAsync(todoDto);
                    }
                    finally
                    {
                        targetId = todoDto.Id;
                    }
                    break;
                case AiDraftType.DayReport:
                    var day = Deserialize<AiDayReportDraft>(draft.ContentJson);
                    var dayDto = new DayWorkReportDto
                    {
                        UserId = userId,
                        RecordTime = day.RecordTime,
                        TodayWork = day.TodayWork.Trim(),
                        TomorrowPlan = day.TomorrowPlan
                    };
                    try
                    {
                        success = await dayWorkReportService.AddAsync(dayDto);
                    }
                    finally
                    {
                        targetId = dayDto.Id;
                    }
                    break;
                case AiDraftType.WeekReport:
                    var week = Deserialize<AiWeekReportDraft>(draft.ContentJson);
                    var weekDto = new WeekWorkReportDto
                    {
                        UserId = userId,
                        StartTime = week.StartTime,
                        EndTime = week.EndTime,
                        WeekWork = week.WeekWork.Trim(),
                        NextWeekPlan = week.NextWeekPlan
                    };
                    try
                    {
                        success = await weekWorkReportService.AddAsync(weekDto);
                    }
                    finally
                    {
                        targetId = weekDto.Id;
                    }
                    break;
                case AiDraftType.MonthReport:
                    var month = Deserialize<AiMonthReportDraft>(draft.ContentJson);
                    var monthDto = new MonthWorkReportDto
                    {
                        UserId = userId,
                        StartTime = month.StartTime,
                        EndTime = month.EndTime,
                        MonthWork = month.MonthWork.Trim(),
                        NextMonthPlan = month.NextMonthPlan
                    };
                    try
                    {
                        success = await monthWorkReportService.AddAsync(monthDto);
                    }
                    finally
                    {
                        targetId = monthDto.Id;
                    }
                    break;
                default:
                    throw new ExplicitException("不支持的草稿类型");
            }
        }
        catch (Exception exception)
        {
            if (targetId > 0)
            {
                throw new TargetCreationUncertainException(exception);
            }
            throw;
        }
        if (!success)
        {
            if (targetId > 0)
            {
                throw new TargetCreationUncertainException();
            }
            throw new ExplicitException("业务数据保存失败");
        }
        if (targetId < 1)
        {
            throw new ExplicitException("业务数据标识无效");
        }
        return targetId;
    }

    private async Task<AiDraftEntity> GetOwnedAsync(long id, bool includeDeleted = false)
    {
        var draft = await draftRepository.GetAsync(item =>
            item.Id == id &&
            item.TenantId == TenantContextHolder.TenantId &&
            item.UserId == TenantContextHolder.UserId &&
            (includeDeleted || !item.IsDelete));
        if (draft == null)
        {
            throw new ExplicitException("草稿不存在或无权访问");
        }
        return draft;
    }

    private static void EnsurePending(AiDraftEntity draft)
    {
        if (draft.IsDelete || draft.Status == AiDraftStatus.Deleted)
        {
            throw new ExplicitException("草稿已删除");
        }
        if (draft.ExpiresAt <= DateTime.Now || draft.Status == AiDraftStatus.Expired)
        {
            throw new ExplicitException("草稿已过期");
        }
        if (draft.Status != AiDraftStatus.Pending)
        {
            throw new ExplicitException("当前草稿状态不允许操作");
        }
    }

    private static string NormalizeAndValidate(AiDraftType type, string contentJson)
    {
        try
        {
            return type switch
            {
                AiDraftType.Note => ValidateNote(Deserialize<AiNoteDraft>(contentJson)),
                AiDraftType.Todo => ValidateTodo(Deserialize<AiTodoDraft>(contentJson)),
                AiDraftType.DayReport => ValidateDay(Deserialize<AiDayReportDraft>(contentJson)),
                AiDraftType.WeekReport => ValidateWeek(Deserialize<AiWeekReportDraft>(contentJson)),
                AiDraftType.MonthReport => ValidateMonth(Deserialize<AiMonthReportDraft>(contentJson)),
                _ => throw new ExplicitException("不支持的草稿类型")
            };
        }
        catch (JsonException)
        {
            throw new ExplicitException("草稿内容格式无效");
        }
    }

    private static string ValidateNote(AiNoteDraft value)
    {
        RequireLength(value.Title, 1, 200, "笔记标题");
        RequireLength(value.ContentMarkdown, 1, 5000, "笔记内容");
        return JsonSerializer.Serialize(value with { Title = value.Title.Trim() }, JsonOptions);
    }

    private static string ValidateTodo(AiTodoDraft value)
    {
        RequireLength(value.Name, 1, 500, "待办名称");
        if (value.CategoryId < 1 || value.Priority is < 1 or > 3)
        {
            throw new ExplicitException("待办分类或优先级无效");
        }
        return JsonSerializer.Serialize(value with { Name = value.Name.Trim() }, JsonOptions);
    }

    private static string ValidateDay(AiDayReportDraft value)
    {
        RequireDate(value.RecordTime, "日报日期");
        RequireLength(value.TodayWork, 1, 2000, "今日工作");
        RequireOptionalLength(value.TomorrowPlan, 2000, "明日计划");
        return JsonSerializer.Serialize(value with { TodayWork = value.TodayWork.Trim() }, JsonOptions);
    }

    private static string ValidateWeek(AiWeekReportDraft value)
    {
        RequireRange(value.StartTime, value.EndTime, "周报日期");
        RequireLength(value.WeekWork, 1, 2000, "本周工作");
        RequireOptionalLength(value.NextWeekPlan, 2000, "下周计划");
        return JsonSerializer.Serialize(value with { WeekWork = value.WeekWork.Trim() }, JsonOptions);
    }

    private static string ValidateMonth(AiMonthReportDraft value)
    {
        RequireRange(value.StartTime, value.EndTime, "月报日期");
        RequireLength(value.MonthWork, 1, 2000, "本月工作");
        RequireOptionalLength(value.NextMonthPlan, 2000, "下月计划");
        return JsonSerializer.Serialize(value with { MonthWork = value.MonthWork.Trim() }, JsonOptions);
    }

    private static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
               ?? throw new JsonException();
    }

    private static void RequireLength(string? value, int min, int max, string name)
    {
        var length = value?.Trim().Length ?? 0;
        if (length < min || length > max)
        {
            throw new ExplicitException($"{name}长度无效");
        }
    }

    private static void RequireOptionalLength(string? value, int max, string name)
    {
        if (value?.Length > max)
        {
            throw new ExplicitException($"{name}长度无效");
        }
    }

    private static void RequireDate(DateTime value, string name)
    {
        if (value == default)
        {
            throw new ExplicitException($"{name}无效");
        }
    }

    private sealed class TargetCreationUncertainException(Exception? innerException = null)
        : Exception("目标业务数据可能已创建", innerException);

    private static void RequireRange(DateTime start, DateTime end, string name)
    {
        if (start == default || end == default || start > end)
        {
            throw new ExplicitException($"{name}无效");
        }
    }

    private static System.Linq.Expressions.Expression<Func<AiDraftEntity, bool>>
        OwnedPendingPredicate(long id)
    {
        return item =>
            item.Id == id &&
            item.TenantId == TenantContextHolder.TenantId &&
            item.UserId == TenantContextHolder.UserId &&
            item.Status == AiDraftStatus.Pending &&
            !item.IsDelete &&
            item.ExpiresAt > DateTime.Now;
    }

    private static AiDraftDto ToDto(AiDraftEntity entity)
    {
        return new AiDraftDto
        {
            Id = entity.Id,
            ConversationId = entity.ConversationId,
            MessageId = entity.MessageId,
            DraftType = entity.DraftType,
            ContentJson = entity.ContentJson,
            ExpiresAt = entity.ExpiresAt,
            ConfirmationStartedAt = entity.ConfirmationStartedAt,
            ConfirmedTargetId = entity.ConfirmedTargetId,
            Status = entity.Status
        };
    }

    private static AiDraftConfirmResultDto ToConfirmResult(AiDraftEntity entity)
    {
        return new AiDraftConfirmResultDto
        {
            DraftId = entity.Id,
            DraftType = entity.DraftType,
            ConfirmedTargetId = entity.ConfirmedTargetId!.Value
        };
    }
}
