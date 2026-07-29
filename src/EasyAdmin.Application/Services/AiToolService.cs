using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Ai;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Sean.Core.DbRepository;
using Sean.Core.DbRepository.Extensions;
using Sean.Core.DbRepository.Util;

namespace EasyAdmin.Application.Services;

/// <summary>
/// AI业务工具服务实现
/// </summary>
public sealed class AiToolService(
    INoteRepository noteRepository,
    ITodoItemRepository todoRepository,
    IDayWorkReportRepository dayReportRepository,
    IWeekWorkReportRepository weekReportRepository,
    IMonthWorkReportRepository monthReportRepository,
    IUserNotificationRepository userNotificationRepository,
    IMenuService menuService) : IAiToolService
{
    private const int DefaultLimit = 10;
    private const int MaxLimit = 20;
    private const int MaxExcerptLength = 500;
    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new("\\s+", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    /// <inheritdoc />
    public IReadOnlyList<AiToolDefinition> GetDefinitions()
    {
        return
        [
            Definition("search_my_notes", "搜索当前用户未受保护的笔记", """
                {"type":"object","properties":{"keyword":{"type":"string"},"startDate":{"type":"string","format":"date"},"endDate":{"type":"string","format":"date"},"limit":{"type":"integer","minimum":1,"maximum":20}},"additionalProperties":false}
                """),
            Definition("query_my_todos", "查询当前用户待办事项", """
                {"type":"object","properties":{"keyword":{"type":"string"},"done":{"type":"boolean"},"priority":{"type":"integer","minimum":1,"maximum":3},"limit":{"type":"integer","minimum":1,"maximum":20}},"additionalProperties":false}
                """),
            Definition("query_my_work_reports", "查询当前用户日报周报或月报", """
                {"type":"object","required":["reportType"],"properties":{"reportType":{"type":"string","enum":["day","week","month"]},"startDate":{"type":"string","format":"date"},"endDate":{"type":"string","format":"date"},"limit":{"type":"integer","minimum":1,"maximum":20}},"additionalProperties":false}
                """),
            Definition("query_visible_notifications", "查询当前用户可见通知", """
                {"type":"object","properties":{"keyword":{"type":"string"},"startDate":{"type":"string","format":"date"},"endDate":{"type":"string","format":"date"},"limit":{"type":"integer","minimum":1,"maximum":20}},"additionalProperties":false}
                """),
            Definition("query_visible_menus", "查询当前用户可见菜单", """
                {"type":"object","properties":{"keyword":{"type":"string"},"limit":{"type":"integer","minimum":1,"maximum":20}},"additionalProperties":false}
                """)
        ];
    }

    /// <inheritdoc />
    public Task<AiToolExecutionResult> ExecuteAsync(string name, string argumentsJson)
    {
        return name switch
        {
            "search_my_notes" => SearchNotesAsync(Parse<SearchArgs>(argumentsJson)),
            "query_my_todos" => QueryTodosAsync(Parse<TodoArgs>(argumentsJson)),
            "query_my_work_reports" => QueryReportsAsync(Parse<ReportArgs>(argumentsJson)),
            "query_visible_notifications" => QueryNotificationsAsync(Parse<SearchArgs>(argumentsJson)),
            "query_visible_menus" => QueryMenusAsync(Parse<MenuArgs>(argumentsJson)),
            _ => throw new ExplicitException("不支持的AI工具")
        };
    }

    /// <inheritdoc />
    public async Task<string> ResolveSourceAsync(AiSourceType sourceType, long sourceId)
    {
        string? route = sourceType switch
        {
            AiSourceType.Note => await ResolveNoteAsync(sourceId),
            AiSourceType.Todo => await ResolveTodoAsync(sourceId),
            AiSourceType.DayReport => await ResolveDayReportAsync(sourceId),
            AiSourceType.WeekReport => await ResolveWeekReportAsync(sourceId),
            AiSourceType.MonthReport => await ResolveMonthReportAsync(sourceId),
            AiSourceType.Notification => await ResolveNotificationAsync(sourceId),
            AiSourceType.Menu => await ResolveMenuAsync(sourceId),
            _ => null
        };
        return route ?? throw new ExplicitException("来源已删除或无权访问");
    }

    private async Task<string?> ResolveNoteAsync(long sourceId)
    {
        var entity = await noteRepository.GetAsync(item =>
            item.Id == sourceId &&
            item.TenantId == TenantContextHolder.TenantId &&
            item.UserId == TenantContextHolder.UserId &&
            !item.IsProtected &&
            !item.IsDelete);
        return entity == null ? null : $"/user/note?openNoteId={entity.Id}";
    }

    private async Task<string?> ResolveTodoAsync(long sourceId)
    {
        var entity = await todoRepository.GetAsync(item =>
            item.Id == sourceId &&
            item.TenantId == TenantContextHolder.TenantId &&
            item.UserId == TenantContextHolder.UserId &&
            !item.IsDelete);
        return entity == null ? null : "/user/todoList";
    }

    private async Task<string?> ResolveDayReportAsync(long sourceId)
    {
        var entity = await dayReportRepository.GetAsync(item =>
            item.Id == sourceId &&
            item.TenantId == TenantContextHolder.TenantId &&
            item.UserId == TenantContextHolder.UserId &&
            !item.IsDelete);
        return entity == null ? null : "/user/dayWorkReport";
    }

    private async Task<string?> ResolveWeekReportAsync(long sourceId)
    {
        var entity = await weekReportRepository.GetAsync(item =>
            item.Id == sourceId &&
            item.TenantId == TenantContextHolder.TenantId &&
            item.UserId == TenantContextHolder.UserId &&
            !item.IsDelete);
        return entity == null ? null : "/user/weekWorkReport";
    }

    private async Task<string?> ResolveMonthReportAsync(long sourceId)
    {
        var entity = await monthReportRepository.GetAsync(item =>
            item.Id == sourceId &&
            item.TenantId == TenantContextHolder.TenantId &&
            item.UserId == TenantContextHolder.UserId &&
            !item.IsDelete);
        return entity == null ? null : "/user/monthWorkReport";
    }

    private async Task<string?> ResolveNotificationAsync(long sourceId)
    {
        var entity = await userNotificationRepository.GetAsync(item =>
            item.Id == sourceId &&
            item.TenantId == TenantContextHolder.TenantId &&
            item.UserId == TenantContextHolder.UserId &&
            item.NoticeState == CommonState.Enable &&
            !item.IsDelete);
        return entity == null ? null : $"/user/message?openMessageId={entity.Id}";
    }

    private async Task<string?> ResolveMenuAsync(long sourceId)
    {
        var menuTree = await menuService.GetMenuTreeAsync(
            TenantContextHolder.UserId,
            new MenuListReqDto());
        return FlattenMenus(menuTree)
            .FirstOrDefault(item =>
                item.Id == sourceId &&
                item.State == CommonState.Enable &&
                !string.IsNullOrWhiteSpace(item.Path))
            ?.Path;
    }

    private async Task<AiToolExecutionResult> SearchNotesAsync(SearchArgs args)
    {
        ValidateDateRange(args.StartDate, args.EndDate);
        var limit = NormalizeLimit(args.Limit);
        var order = Descending<NoteEntity>(item => item.UpdateTime);
        var page = await noteRepository.PageQueryAsync(
            WhereExpressionUtil.Create<NoteEntity>(item =>
                    item.TenantId == TenantContextHolder.TenantId &&
                    item.UserId == TenantContextHolder.UserId &&
                    !item.IsProtected &&
                    !item.IsDelete)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(args.Keyword), item =>
                    item.Title.Contains(args.Keyword!) ||
                    (item.ContentText != null && item.ContentText.Contains(args.Keyword!)) ||
                    (item.Summary != null && item.Summary.Contains(args.Keyword!)))
                .AndAlsoIF(args.StartDate.HasValue, item => item.UpdateTime >= args.StartDate)
                .AndAlsoIF(args.EndDate.HasValue, item => item.UpdateTime < args.EndDate!.Value.Date.AddDays(1)),
            order,
            1,
            limit);

        var sources = (page.List ?? []).Select(item => Source(
            AiSourceType.Note,
            item.Id,
            item.Title,
            item.UpdateTime,
            FirstNonempty(item.ContentText, item.Summary),
            $"/user/note?openNoteId={item.Id}")).ToList();
        return Result(sources);
    }

    private async Task<AiToolExecutionResult> QueryTodosAsync(TodoArgs args)
    {
        if (args.Priority is < 1 or > 3)
        {
            throw new ExplicitException("待办优先级无效");
        }

        var limit = NormalizeLimit(args.Limit);
        var order = Descending<TodoItemEntity>(item => item.UpdateTime);
        var page = await todoRepository.PageQueryAsync(
            WhereExpressionUtil.Create<TodoItemEntity>(item =>
                    item.TenantId == TenantContextHolder.TenantId &&
                    item.UserId == TenantContextHolder.UserId &&
                    !item.IsDelete)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(args.Keyword), item => item.Name.Contains(args.Keyword!))
                .AndAlsoIF(args.Done.HasValue, item => item.Done == args.Done)
                .AndAlsoIF(args.Priority.HasValue, item => item.Priority == args.Priority),
            order,
            1,
            limit);

        var sources = (page.List ?? []).Select(item => Source(
            AiSourceType.Todo,
            item.Id,
            item.Name,
            item.UpdateTime,
            $"完成：{(item.Done ? "是" : "否")}；优先级：{item.Priority}",
            "/user/todoList")).ToList();
        return Result(sources);
    }

    private Task<AiToolExecutionResult> QueryReportsAsync(ReportArgs args)
    {
        ValidateDateRange(args.StartDate, args.EndDate);
        return args.ReportType?.Trim().ToLowerInvariant() switch
        {
            "day" => QueryDayReportsAsync(args),
            "week" => QueryWeekReportsAsync(args),
            "month" => QueryMonthReportsAsync(args),
            _ => throw new ExplicitException("工作报告类型无效")
        };
    }

    private async Task<AiToolExecutionResult> QueryDayReportsAsync(ReportArgs args)
    {
        var page = await dayReportRepository.PageQueryAsync(
            WhereExpressionUtil.Create<DayWorkReportEntity>(item =>
                    item.TenantId == TenantContextHolder.TenantId &&
                    item.UserId == TenantContextHolder.UserId &&
                    !item.IsDelete)
                .AndAlsoIF(args.StartDate.HasValue, item => item.RecordTime >= args.StartDate)
                .AndAlsoIF(args.EndDate.HasValue, item => item.RecordTime < args.EndDate!.Value.Date.AddDays(1)),
            Descending<DayWorkReportEntity>(item => item.RecordTime),
            1,
            NormalizeLimit(args.Limit));
        var sources = (page.List ?? []).Select(item => Source(
            AiSourceType.DayReport,
            item.Id,
            $"{item.RecordTime:yyyy-MM-dd} 日报",
            item.RecordTime,
            FirstNonempty(item.TodayWork, item.TomorrowPlan),
            "/user/dayWorkReport")).ToList();
        return Result(sources);
    }

    private async Task<AiToolExecutionResult> QueryWeekReportsAsync(ReportArgs args)
    {
        var page = await weekReportRepository.PageQueryAsync(
            WhereExpressionUtil.Create<WeekWorkReportEntity>(item =>
                    item.TenantId == TenantContextHolder.TenantId &&
                    item.UserId == TenantContextHolder.UserId &&
                    !item.IsDelete)
                .AndAlsoIF(args.StartDate.HasValue, item => item.EndTime >= args.StartDate)
                .AndAlsoIF(args.EndDate.HasValue, item => item.StartTime < args.EndDate!.Value.Date.AddDays(1)),
            Descending<WeekWorkReportEntity>(item => item.StartTime),
            1,
            NormalizeLimit(args.Limit));
        var sources = (page.List ?? []).Select(item => Source(
            AiSourceType.WeekReport,
            item.Id,
            $"{item.StartTime:yyyy-MM-dd} 至 {item.EndTime:yyyy-MM-dd} 周报",
            item.StartTime,
            FirstNonempty(item.WeekWork, item.NextWeekPlan),
            "/user/weekWorkReport")).ToList();
        return Result(sources);
    }

    private async Task<AiToolExecutionResult> QueryMonthReportsAsync(ReportArgs args)
    {
        var page = await monthReportRepository.PageQueryAsync(
            WhereExpressionUtil.Create<MonthWorkReportEntity>(item =>
                    item.TenantId == TenantContextHolder.TenantId &&
                    item.UserId == TenantContextHolder.UserId &&
                    !item.IsDelete)
                .AndAlsoIF(args.StartDate.HasValue, item => item.EndTime >= args.StartDate)
                .AndAlsoIF(args.EndDate.HasValue, item => item.StartTime < args.EndDate!.Value.Date.AddDays(1)),
            Descending<MonthWorkReportEntity>(item => item.StartTime),
            1,
            NormalizeLimit(args.Limit));
        var sources = (page.List ?? []).Select(item => Source(
            AiSourceType.MonthReport,
            item.Id,
            $"{item.StartTime:yyyy-MM} 月报",
            item.StartTime,
            FirstNonempty(item.MonthWork, item.NextMonthPlan),
            "/user/monthWorkReport")).ToList();
        return Result(sources);
    }

    private async Task<AiToolExecutionResult> QueryNotificationsAsync(SearchArgs args)
    {
        ValidateDateRange(args.StartDate, args.EndDate);
        var page = await userNotificationRepository.PageQueryAsync(
            WhereExpressionUtil.Create<UserNotificationEntity>(item =>
                    item.TenantId == TenantContextHolder.TenantId &&
                    item.UserId == TenantContextHolder.UserId &&
                    item.NoticeState == CommonState.Enable &&
                    !item.IsDelete)
                .AndAlsoIF(!string.IsNullOrWhiteSpace(args.Keyword), item =>
                    (item.Title != null && item.Title.Contains(args.Keyword!)) ||
                    (item.Content != null && item.Content.Contains(args.Keyword!)))
                .AndAlsoIF(args.StartDate.HasValue, item => item.SendTime >= args.StartDate)
                .AndAlsoIF(args.EndDate.HasValue, item => item.SendTime < args.EndDate!.Value.Date.AddDays(1)),
            Descending<UserNotificationEntity>(item => item.CreateTime),
            1,
            NormalizeLimit(args.Limit));
        var sources = (page.List ?? []).Select(item => Source(
            AiSourceType.Notification,
            item.Id,
            item.Title,
            item.SendTime,
            item.Content,
            $"/user/message?openMessageId={item.Id}")).ToList();
        return Result(sources);
    }

    private async Task<AiToolExecutionResult> QueryMenusAsync(MenuArgs args)
    {
        var menuTree = await menuService.GetMenuTreeAsync(
            TenantContextHolder.UserId,
            new MenuListReqDto());
        var keyword = args.Keyword?.Trim();
        var menus = FlattenMenus(menuTree)
            .Where(item =>
                item.State == CommonState.Enable &&
                !string.IsNullOrWhiteSpace(item.Path) &&
                (string.IsNullOrWhiteSpace(keyword) ||
                 item.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 item.Path!.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Take(NormalizeLimit(args.Limit))
            .ToList();
        var sources = menus.Select(item => Source(
            AiSourceType.Menu,
            item.Id,
            item.Title,
            null,
            item.Path,
            item.Path!)).ToList();
        return Result(sources);
    }

    private static AiToolDefinition Definition(string name, string description, string schema)
    {
        using var document = JsonDocument.Parse(schema);
        return new AiToolDefinition(name, description, document.RootElement.Clone());
    }

    private static T Parse<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                   ?? throw new ExplicitException("AI工具参数无效");
        }
        catch (JsonException)
        {
            throw new ExplicitException("AI工具参数无效");
        }
    }

    private static void ValidateDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue)
        {
            return;
        }
        if (endDate.Value.Date < startDate.Value.Date ||
            (endDate.Value.Date - startDate.Value.Date).TotalDays > 366)
        {
            throw new ExplicitException("查询日期范围必须在366天以内");
        }
    }

    private static int NormalizeLimit(int? limit)
    {
        return Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);
    }

    private static OrderByCondition Descending<T>(System.Linq.Expressions.Expression<Func<T, object>> expression)
    {
        return OrderByConditionBuilder<T>.Build(OrderByType.Desc, expression);
    }

    private static AiSourceCandidate Source(
        AiSourceType type,
        long recordId,
        string? title,
        DateTime? date,
        string? excerpt,
        string route)
    {
        return new AiSourceCandidate
        {
            SourceType = type,
            RecordId = recordId,
            Title = CleanText(title),
            Date = date,
            Excerpt = Truncate(CleanText(excerpt), MaxExcerptLength),
            Route = route
        };
    }

    private static AiToolExecutionResult Result(IReadOnlyList<AiSourceCandidate> sources)
    {
        return new AiToolExecutionResult
        {
            Json = JsonSerializer.Serialize(sources.Select(item => new
            {
                type = item.SourceType.ToString(),
                id = item.RecordId,
                title = item.Title,
                date = item.Date,
                excerpt = item.Excerpt,
                route = item.Route
            }), JsonOptions),
            Sources = sources
        };
    }

    private static string FirstNonempty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private static string CleanText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }
        var decoded = WebUtility.HtmlDecode(HtmlTagRegex.Replace(value, " "));
        return WhitespaceRegex.Replace(decoded, " ").Trim();
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static IEnumerable<MenuEntity> FlattenMenus(IEnumerable<MenuEntity> menus)
    {
        var seen = new HashSet<long>();
        var stack = new Stack<MenuEntity>(menus.Reverse());
        while (stack.Count > 0)
        {
            var menu = stack.Pop();
            if (!seen.Add(menu.Id))
            {
                continue;
            }
            yield return menu;
            if (menu.Children == null)
            {
                continue;
            }
            foreach (var child in menu.Children.AsEnumerable().Reverse())
            {
                stack.Push(child);
            }
        }
    }

    private sealed class SearchArgs
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 返回数量
        /// </summary>
        public int? Limit { get; set; }
    }

    private sealed class TodoArgs
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool? Done { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// 返回数量
        /// </summary>
        public int? Limit { get; set; }
    }

    private sealed class ReportArgs
    {
        /// <summary>
        /// 报告类型
        /// </summary>
        public string? ReportType { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 返回数量
        /// </summary>
        public int? Limit { get; set; }
    }

    private sealed class MenuArgs
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 返回数量
        /// </summary>
        public int? Limit { get; set; }
    }
}
