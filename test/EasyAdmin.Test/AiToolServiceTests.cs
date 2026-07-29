using System.Linq.Expressions;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Moq;
using Sean.Core.DbRepository;

namespace EasyAdmin.Test;

[TestClass]
public class AiToolServiceTests
{
    [TestInitialize]
    public void Initialize()
    {
        TenantContextHolder.UserInfo = new JwtUserModel { TenantId = 7, UserId = 11 };
    }

    [TestCleanup]
    public void Cleanup()
    {
        TenantContextHolder.Clear();
    }

    [TestMethod]
    public async Task SearchNotes_EnforcesOwnerProtectionAndBounds()
    {
        var mocks = new ToolMocks();
        Expression<Func<NoteEntity, bool>>? predicate = null;
        var pageSize = 0;
        mocks.Notes
            .Setup(item => item.PageQueryAsync(
                It.IsAny<Expression<Func<NoteEntity, bool>>>(),
                It.IsAny<OrderByCondition>(),
                1,
                It.IsAny<int>(),
                It.IsAny<Expression<Func<NoteEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<NoteEntity, bool>>, OrderByCondition, int, int, Expression<Func<NoteEntity, object>>?, bool>(
                (value, _, _, size, _, _) =>
                {
                    predicate = value;
                    pageSize = size;
                })
            .ReturnsAsync(new PageQueryResult<NoteEntity>
            {
                Total = 1,
                List =
                [
                    new NoteEntity
                    {
                        Id = 21,
                        TenantId = 7,
                        UserId = 11,
                        Title = "AI note",
                        ContentText = new string('x', 700),
                        UpdateTime = new DateTime(2026, 7, 1)
                    }
                ]
            });

        var result = await mocks.CreateService().ExecuteAsync(
            "search_my_notes",
            """{"keyword":"AI","limit":100}""");

        Assert.AreEqual(20, pageSize);
        var compiled = predicate!.Compile();
        Assert.IsTrue(compiled(new NoteEntity
        {
            TenantId = 7,
            UserId = 11,
            Title = "AI",
            IsProtected = false
        }));
        Assert.IsFalse(compiled(new NoteEntity { TenantId = 8, UserId = 11, Title = "AI" }));
        Assert.IsFalse(compiled(new NoteEntity { TenantId = 7, UserId = 12, Title = "AI" }));
        Assert.IsFalse(compiled(new NoteEntity
        {
            TenantId = 7,
            UserId = 11,
            Title = "AI",
            IsProtected = true
        }));
        Assert.AreEqual(1, result.Sources.Count);
        Assert.AreEqual(AiSourceType.Note, result.Sources[0].SourceType);
        Assert.IsTrue(result.Sources[0].Excerpt.Length <= 500);
        Assert.IsFalse(result.Json.Contains(new string('x', 501), StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task QueryTodos_EnforcesOwnerAndRequestedFilters()
    {
        var mocks = new ToolMocks();
        Expression<Func<TodoItemEntity, bool>>? predicate = null;
        mocks.Todos
            .Setup(item => item.PageQueryAsync(
                It.IsAny<Expression<Func<TodoItemEntity, bool>>>(),
                It.IsAny<OrderByCondition>(),
                1,
                10,
                It.IsAny<Expression<Func<TodoItemEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<TodoItemEntity, bool>>, OrderByCondition, int, int, Expression<Func<TodoItemEntity, object>>?, bool>(
                (value, _, _, _, _, _) => predicate = value)
            .ReturnsAsync(new PageQueryResult<TodoItemEntity>());

        await mocks.CreateService().ExecuteAsync(
            "query_my_todos",
            """{"keyword":"ship","done":false,"priority":3,"limit":10}""");

        var compiled = predicate!.Compile();
        Assert.IsTrue(compiled(new TodoItemEntity
        {
            TenantId = 7,
            UserId = 11,
            Name = "ship feature",
            Done = false,
            Priority = 3
        }));
        Assert.IsFalse(compiled(new TodoItemEntity
        {
            TenantId = 7,
            UserId = 12,
            Name = "ship feature",
            Priority = 3
        }));
    }

    [TestMethod]
    public async Task QueryReports_RejectsRangesLongerThan366Days()
    {
        var mocks = new ToolMocks();

        await Assert.ThrowsExactlyAsync<ExplicitException>(() =>
            mocks.CreateService().ExecuteAsync(
                "query_my_work_reports",
                """{"reportType":"day","startDate":"2024-01-01","endDate":"2025-01-02"}"""));
    }

    [TestMethod]
    public async Task QueryNotifications_UsesRecipientVisibilityPredicate()
    {
        var mocks = new ToolMocks();
        Expression<Func<UserNotificationEntity, bool>>? predicate = null;
        mocks.Notifications
            .Setup(item => item.PageQueryAsync(
                It.IsAny<Expression<Func<UserNotificationEntity, bool>>>(),
                It.IsAny<OrderByCondition>(),
                1,
                10,
                It.IsAny<Expression<Func<UserNotificationEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<UserNotificationEntity, bool>>, OrderByCondition, int, int, Expression<Func<UserNotificationEntity, object>>?, bool>(
                (value, _, _, _, _, _) => predicate = value)
            .ReturnsAsync(new PageQueryResult<UserNotificationEntity>());

        await mocks.CreateService().ExecuteAsync(
            "query_visible_notifications",
            """{"keyword":"release","limit":10}""");

        var compiled = predicate!.Compile();
        Assert.IsTrue(compiled(new UserNotificationEntity
        {
            TenantId = 7,
            UserId = 11,
            Title = "release",
            NoticeState = CommonState.Enable
        }));
        Assert.IsFalse(compiled(new UserNotificationEntity
        {
            TenantId = 7,
            UserId = 12,
            Title = "release",
            NoticeState = CommonState.Enable
        }));
        Assert.IsFalse(compiled(new UserNotificationEntity
        {
            TenantId = 7,
            UserId = 11,
            Title = "release",
            NoticeState = CommonState.Disable
        }));
    }

    [TestMethod]
    public async Task QueryMenus_UsesResolvedCurrentUserMenuTree()
    {
        var mocks = new ToolMocks();
        mocks.Menus
            .Setup(item => item.GetMenuTreeAsync(
                11,
                It.Is<MenuListReqDto>(request => !request.All && !request.IncludeTopMenu)))
            .ReturnsAsync(
            [
                new MenuEntity
                {
                    Id = 31,
                    Title = "Notes",
                    Path = "/user/note",
                    State = CommonState.Enable
                }
            ]);

        var result = await mocks.CreateService().ExecuteAsync(
            "query_visible_menus",
            """{"keyword":"Notes","limit":10}""");

        Assert.AreEqual(1, result.Sources.Count);
        Assert.AreEqual(AiSourceType.Menu, result.Sources[0].SourceType);
        Assert.AreEqual("/user/note", result.Sources[0].Route);
    }

    [TestMethod]
    public async Task ResolveSourceAsync_RechecksCurrentOwnerBeforeReturningRoute()
    {
        var mocks = new ToolMocks();
        Expression<Func<NoteEntity, bool>>? predicate = null;
        mocks.Notes
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<NoteEntity, bool>>>(),
                It.IsAny<Expression<Func<NoteEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<NoteEntity, bool>>, Expression<Func<NoteEntity, object>>?, bool>(
                (value, _, _) => predicate = value)
            .ReturnsAsync(new NoteEntity { Id = 41, TenantId = 7, UserId = 11 });

        var route = await mocks.CreateService().ResolveSourceAsync(AiSourceType.Note, 41);

        Assert.AreEqual("/user/note?openNoteId=41", route);
        var compiled = predicate!.Compile();
        Assert.IsTrue(compiled(new NoteEntity { Id = 41, TenantId = 7, UserId = 11 }));
        Assert.IsFalse(compiled(new NoteEntity { Id = 41, TenantId = 7, UserId = 12 }));
        Assert.IsFalse(compiled(new NoteEntity { Id = 41, TenantId = 7, UserId = 11, IsDelete = true }));
    }

    [TestMethod]
    public async Task ResolveSourceAsync_RejectsMissingOrInaccessibleSource()
    {
        var mocks = new ToolMocks();
        mocks.Notes
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<NoteEntity, bool>>>(),
                It.IsAny<Expression<Func<NoteEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((NoteEntity)null!);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() =>
            mocks.CreateService().ResolveSourceAsync(AiSourceType.Note, 41));
    }

    [TestMethod]
    public async Task ExecuteAsync_RejectsUnknownToolAndInvalidJson()
    {
        var service = new ToolMocks().CreateService();

        await Assert.ThrowsExactlyAsync<ExplicitException>(() =>
            service.ExecuteAsync("unknown_tool", "{}"));
        await Assert.ThrowsExactlyAsync<ExplicitException>(() =>
            service.ExecuteAsync("search_my_notes", "{bad-json}"));
        await Assert.ThrowsExactlyAsync<ExplicitException>(() =>
            service.ExecuteAsync("search_my_notes", """{"tenantId":99}"""));
    }

    private sealed class ToolMocks
    {
        public Mock<INoteRepository> Notes { get; } = new();
        public Mock<ITodoItemRepository> Todos { get; } = new();
        public Mock<IDayWorkReportRepository> DayReports { get; } = new();
        public Mock<IWeekWorkReportRepository> WeekReports { get; } = new();
        public Mock<IMonthWorkReportRepository> MonthReports { get; } = new();
        public Mock<IUserNotificationRepository> Notifications { get; } = new();
        public Mock<IMenuService> Menus { get; } = new();

        public AiToolService CreateService()
        {
            return new AiToolService(
                Notes.Object,
                Todos.Object,
                DayReports.Object,
                WeekReports.Object,
                MonthReports.Object,
                Notifications.Object,
                Menus.Object);
        }
    }
}
