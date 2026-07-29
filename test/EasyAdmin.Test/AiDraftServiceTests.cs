using System.Data;
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

namespace EasyAdmin.Test;

[TestClass]
public class AiDraftServiceTests
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
    public async Task CreateFromModelAsync_ValidatesOwnershipAndCreatesOneHourDraft()
    {
        var fixture = new DraftFixture();
        fixture.Conversations
            .Setup(item => item.GetOwnedAsync(5))
            .ReturnsAsync(new AiConversationEntity { Id = 5 });
        AiDraftEntity? added = null;
        fixture.Drafts
            .Setup(item => item.AddAsync(
                It.IsAny<AiDraftEntity>(),
                false,
                null,
                null))
            .Callback<AiDraftEntity, bool, Expression<Func<AiDraftEntity, object>>?, IDbTransaction?>(
                (entity, _, _, _) =>
                {
                    entity.Id = 101;
                    added = entity;
                })
            .ReturnsAsync(true);
        var before = DateTime.Now;

        var result = await fixture.Service.CreateFromModelAsync(
            5,
            9,
            AiDraftType.Note,
            """{"title":"Plan","contentMarkdown":"Details"}""");

        Assert.AreEqual(101, result.Id);
        Assert.AreEqual(7, added!.TenantId);
        Assert.AreEqual(11, added.UserId);
        Assert.AreEqual(AiDraftStatus.Pending, added.Status);
        Assert.IsTrue(added.ExpiresAt >= before.AddMinutes(59));
        fixture.Conversations.Verify(item => item.GetOwnedAsync(5), Times.Once);
    }

    [TestMethod]
    public async Task CreateFromModelAsync_RejectsInvalidPayloadsForEveryType()
    {
        var fixture = new DraftFixture();
        fixture.Conversations
            .Setup(item => item.GetOwnedAsync(5))
            .ReturnsAsync(new AiConversationEntity { Id = 5 });
        var invalid = new[]
        {
            (AiDraftType.Note, """{"title":"","contentMarkdown":"x"}"""),
            (AiDraftType.Todo, """{"name":"x","categoryId":0,"priority":1}"""),
            (AiDraftType.DayReport, """{"recordTime":"0001-01-01","todayWork":"x"}"""),
            (AiDraftType.WeekReport, """{"startTime":"2026-07-02","endTime":"2026-07-01","weekWork":"x"}"""),
            (AiDraftType.MonthReport, """{"startTime":"2026-07-02","endTime":"2026-07-01","monthWork":"x"}""")
        };

        foreach (var item in invalid)
        {
            await Assert.ThrowsExactlyAsync<ExplicitException>(() =>
                fixture.Service.CreateFromModelAsync(5, 9, item.Item1, item.Item2));
        }
        fixture.Drafts.Verify(item => item.AddAsync(
            It.IsAny<AiDraftEntity>(),
            It.IsAny<bool>(),
            It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
            It.IsAny<IDbTransaction>()), Times.Never);
    }

    [TestMethod]
    public async Task ConfirmAsync_IsIdempotentAndCallsBusinessServiceOnce()
    {
        var fixture = new DraftFixture();
        var draft = fixture.NoteDraft();
        fixture.Drafts
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(draft);
        fixture.Drafts
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiDraftEntity>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(1);
        fixture.Notes
            .Setup(item => item.AddAsync(It.IsAny<NoteUpdateDto>()))
            .Callback<NoteUpdateDto>(dto => dto.Id = 501)
            .ReturnsAsync(true);

        var first = await fixture.Service.ConfirmAsync(20);
        var second = await fixture.Service.ConfirmAsync(20);

        Assert.AreEqual(501, first.ConfirmedTargetId);
        Assert.AreEqual(501, second.ConfirmedTargetId);
        fixture.Notes.Verify(item => item.AddAsync(It.IsAny<NoteUpdateDto>()), Times.Once);
    }

    [TestMethod]
    public async Task ConfirmAsync_BusinessFailureRestoresPending()
    {
        var fixture = new DraftFixture();
        fixture.Drafts
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(fixture.NoteDraft());
        var statusUpdates = new List<AiDraftStatus>();
        fixture.Drafts
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiDraftEntity>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<IDbTransaction>()))
            .Callback<AiDraftEntity, Expression<Func<AiDraftEntity, object>>?,
                Expression<Func<AiDraftEntity, bool>>?, IDbTransaction?>(
                (entity, _, _, _) => statusUpdates.Add(entity.Status))
            .ReturnsAsync(1);
        fixture.Notes
            .Setup(item => item.AddAsync(It.IsAny<NoteUpdateDto>()))
            .ThrowsAsync(new ExplicitException("保存失败"));

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => fixture.Service.ConfirmAsync(20));

        CollectionAssert.AreEqual(
            new[] { AiDraftStatus.Confirming, AiDraftStatus.Pending },
            statusUpdates);
    }

    [TestMethod]
    public async Task ConfirmAsync_PostCommitFailureLeavesDraftConfirming()
    {
        var fixture = new DraftFixture();
        fixture.Drafts
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(fixture.NoteDraft());
        var statusUpdates = new List<AiDraftStatus>();
        fixture.Drafts
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiDraftEntity>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<IDbTransaction>()))
            .Callback<AiDraftEntity, Expression<Func<AiDraftEntity, object>>?,
                Expression<Func<AiDraftEntity, bool>>?, IDbTransaction?>(
                (entity, _, _, _) => statusUpdates.Add(entity.Status))
            .ReturnsAsync(1);
        fixture.Notes
            .Setup(item => item.AddAsync(It.IsAny<NoteUpdateDto>()))
            .Returns<NoteUpdateDto>(dto =>
            {
                dto.Id = 501;
                return Task.FromException<bool>(new ExplicitException("标签刷新失败"));
            });

        var exception = await Assert.ThrowsExactlyAsync<ExplicitException>(
            () => fixture.Service.ConfirmAsync(20));

        Assert.AreEqual("草稿确认状态待核查，请联系管理员", exception.Message);
        CollectionAssert.AreEqual(
            new[] { AiDraftStatus.Confirming },
            statusUpdates);
    }

    [TestMethod]
    public async Task ConfirmAsync_RejectsExpiredDraftWithoutBusinessWrite()
    {
        var fixture = new DraftFixture();
        var draft = fixture.NoteDraft();
        draft.ExpiresAt = DateTime.Now.AddSeconds(-1);
        fixture.Drafts
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(draft);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => fixture.Service.ConfirmAsync(20));

        fixture.Notes.Verify(item => item.AddAsync(It.IsAny<NoteUpdateDto>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAndDelete_OnlyOperateOnOwnedPendingDraft()
    {
        var fixture = new DraftFixture();
        fixture.Drafts
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(fixture.NoteDraft());
        fixture.Drafts
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiDraftEntity>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(1);

        var updated = await fixture.Service.UpdateAsync(new AiDraftUpdateDto
        {
            Id = 20,
            ContentJson = """{"title":"Updated","contentMarkdown":"Body"}"""
        });
        await fixture.Service.DeleteAsync(20);

        StringAssert.Contains(updated.ContentJson, "Updated");
        fixture.Drafts.Verify(item => item.UpdateAsync(
            It.Is<AiDraftEntity>(entity =>
                entity.Status == AiDraftStatus.Deleted && entity.IsDelete),
            It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
            It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
            It.IsAny<IDbTransaction>()), Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_RejectsDraftOutsideCurrentOwner()
    {
        var fixture = new DraftFixture();
        fixture.Drafts
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((AiDraftEntity)null!);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => fixture.Service.GetAsync(20));
    }

    private sealed class DraftFixture
    {
        public Mock<IAiDraftRepository> Drafts { get; } = new();
        public Mock<IAiConversationService> Conversations { get; } = new();
        public Mock<INoteService> Notes { get; } = new();
        public Mock<ITodoItemService> Todos { get; } = new();
        public Mock<IDayWorkReportService> Days { get; } = new();
        public Mock<IWeekWorkReportService> Weeks { get; } = new();
        public Mock<IMonthWorkReportService> Months { get; } = new();

        public AiDraftService Service => new(
            Drafts.Object,
            Conversations.Object,
            Notes.Object,
            Todos.Object,
            Days.Object,
            Weeks.Object,
            Months.Object);

        public AiDraftEntity NoteDraft()
        {
            return new AiDraftEntity
            {
                Id = 20,
                TenantId = 7,
                UserId = 11,
                ConversationId = 5,
                MessageId = 9,
                DraftType = AiDraftType.Note,
                ContentJson = """{"title":"Plan","contentMarkdown":"Details"}""",
                ExpiresAt = DateTime.Now.AddMinutes(30),
                Status = AiDraftStatus.Pending
            };
        }
    }
}
