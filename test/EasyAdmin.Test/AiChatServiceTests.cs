using System.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Ai;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sean.Core.DbRepository;

namespace EasyAdmin.Test;

[TestClass]
public class AiChatServiceTests
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
    public async Task StreamAsync_ReservesQuotaExecutesToolsAndPersistsOnlyReferencedSources()
    {
        var fixture = new ChatFixture();
        fixture.SetupConversation();
        fixture.SetupMessagePersistence();
        fixture.Admin
            .Setup(item => item.GetRuntimeConfigAsync(7))
            .ReturnsAsync(new AiRuntimeConfig
            {
                BaseUrl = "https://api.example.com/v1",
                ApiKey = "sk-test",
                Model = "test-model",
                TimeoutSeconds = 30,
                MaxOutputTokens = 1024,
                Temperature = 0.2m,
                DailyRequestLimit = 5
            });
        var callOrder = new List<string>();
        fixture.Quota
            .Setup(item => item.ReserveAsync(7, 5, It.IsAny<DateTime>()))
            .Callback(() => callOrder.Add("quota"))
            .Returns(Task.CompletedTask);
        fixture.Usage
            .Setup(item => item.StartAsync(5, 102, "test-model"))
            .ReturnsAsync(new AiUsageEntity { Id = 301 });
        fixture.Tools
            .Setup(item => item.GetDefinitions())
            .Returns([
                new AiToolDefinition(
                    "search_my_notes",
                    "search",
                    System.Text.Json.JsonDocument.Parse("""{"type":"object"}""").RootElement.Clone())
            ]);
        fixture.Tools
            .Setup(item => item.ExecuteAsync("search_my_notes", """{"keyword":"AI"}"""))
            .ReturnsAsync(new AiToolExecutionResult
            {
                Json = """[{"id":41,"title":"AI note"}]""",
                Sources =
                [
                    new AiSourceCandidate
                    {
                        SourceType = AiSourceType.Note,
                        RecordId = 41,
                        Title = "AI note",
                        Date = new DateTime(2026, 7, 27, 9, 30, 0),
                        Excerpt = "note excerpt",
                        Route = "/user/note?openNoteId=41"
                    }
                ]
            });
        fixture.Model
            .SetupSequence(item => item.StreamAsync(
                It.IsAny<AiModelClientOptions>(),
                It.IsAny<AiModelRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(ModelEvents(
                new AiModelToolCallsCompleted([
                    new AiModelToolCall("call-1", "search_my_notes", """{"keyword":"AI"}""")
                ]),
                new AiModelUsageCompleted(5, 2, 7),
                new AiModelCompleted()))
            .Returns(() =>
            {
                callOrder.Add("provider");
                return ModelEvents(
                    new AiModelTextDelta("Answer [来源:1] [来源:99]"),
                    new AiModelUsageCompleted(8, 4, 12),
                    new AiModelCompleted());
            });

        var events = await CollectAsync(fixture.CreateService().StreamAsync(
            new AiChatRequestDto { ConversationId = 5, Prompt = "Find my AI note" },
            CancellationToken.None));

        CollectionAssert.AreEqual(new[] { "quota", "provider" }, callOrder);
        Assert.IsTrue(events.Any(item => item.Type == "message_started"));
        Assert.IsTrue(events.Any(item => item.Type == "text_delta"));
        Assert.IsTrue(events.Any(item => item.Type == "sources"));
        Assert.IsTrue(events.Any(item => item.Type == "message_completed"));
        Assert.AreEqual("Answer [来源:1] ", fixture.AssistantUpdate!.Content);
        Assert.AreEqual(AiMessageStatus.Completed, fixture.AssistantUpdate.Status);
        Assert.AreEqual(1, fixture.AddedSources.Count);
        Assert.AreEqual(41, fixture.AddedSources[0].SourceId);
        Assert.AreEqual(new DateTime(2026, 7, 27, 9, 30, 0), fixture.AddedSources[0].SourceDate);
        Assert.AreEqual("/user/note?openNoteId=41", fixture.AddedSources[0].Route);
        fixture.Conversations.Verify(item => item.GetOwnedAsync(5), Times.Once);
        fixture.ConversationRepository.Verify(item => item.UpdateAsync(
            It.Is<AiConversationEntity>(entity => entity.Id == 5 && entity.Title == "Find my AI note"),
            It.IsAny<Expression<Func<AiConversationEntity, object>>>(),
            It.IsAny<Expression<Func<AiConversationEntity, bool>>>(),
            It.IsAny<IDbTransaction>()), Times.Once);
        fixture.Usage.Verify(item => item.CompleteAsync(
            301,
            AiUsageStatus.Succeeded,
            13,
            6,
            It.IsAny<long>(),
            null), Times.Once);
    }

    [TestMethod]
    public async Task StreamAsync_RejectsInvalidPromptBeforePersistence()
    {
        var fixture = new ChatFixture();

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => CollectAsync(
            fixture.CreateService().StreamAsync(
                new AiChatRequestDto { ConversationId = 5, Prompt = " " },
                CancellationToken.None)));

        fixture.Messages.Verify(item => item.AddAsync(
            It.IsAny<AiMessageEntity>(),
            It.IsAny<bool>(),
            It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
            It.IsAny<IDbTransaction>()), Times.Never);
    }

    [TestMethod]
    public async Task StreamAsync_DisabledTenantFailsBeforeMessagePersistence()
    {
        var fixture = new ChatFixture();
        fixture.SetupConversation();
        fixture.Admin
            .Setup(item => item.GetRuntimeConfigAsync(7))
            .ThrowsAsync(new ExplicitException("当前租户未启用AI助手"));

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => CollectAsync(
            fixture.CreateService().StreamAsync(
                new AiChatRequestDto { ConversationId = 5, Prompt = "hello" },
                CancellationToken.None)));

        fixture.Messages.Verify(item => item.AddAsync(
            It.IsAny<AiMessageEntity>(),
            It.IsAny<bool>(),
            It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
            It.IsAny<IDbTransaction>()), Times.Never);
        fixture.Quota.Verify(
            item => item.ReserveAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [TestMethod]
    public async Task StreamAsync_DistributedLeaseUnavailableFailsBeforeMessagePersistence()
    {
        var fixture = new ChatFixture();
        fixture.SetupConversation();
        fixture.Admin
            .Setup(item => item.GetRuntimeConfigAsync(7))
            .ReturnsAsync(new AiRuntimeConfig
            {
                BaseUrl = "https://api.example.com/v1",
                ApiKey = "sk-test",
                Model = "test-model",
                TimeoutSeconds = 30,
                MaxOutputTokens = 1024,
                Temperature = 0.2m,
                DailyRequestLimit = 5
            });
        fixture.Coordinator
            .Setup(item => item.TryAcquireAsync(7, 11, 5))
            .ReturnsAsync((string?)null);

        var exception = await Assert.ThrowsExactlyAsync<ExplicitException>(() => CollectAsync(
            fixture.CreateService().StreamAsync(
                new AiChatRequestDto { ConversationId = 5, Prompt = "hello" },
                CancellationToken.None)));

        Assert.AreEqual("当前会话正在生成回复", exception.Message);
        fixture.Messages.Verify(item => item.AddAsync(
            It.IsAny<AiMessageEntity>(),
            It.IsAny<bool>(),
            It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
            It.IsAny<IDbTransaction>()), Times.Never);
    }

    [TestMethod]
    public async Task CancelAsync_RequestsCancellationAcrossInstances()
    {
        var fixture = new ChatFixture();
        fixture.SetupConversation();
        fixture.Coordinator
            .Setup(item => item.RequestCancellationAsync(7, 11, 5))
            .ReturnsAsync(true);

        await fixture.CreateService().CancelAsync(5);

        fixture.Coordinator.Verify(
            item => item.RequestCancellationAsync(7, 11, 5),
            Times.Once);
    }

    [TestMethod]
    public async Task StreamAsync_ProviderFailureFinalizesMessageAndUsageOnce()
    {
        var fixture = new ChatFixture();
        fixture.SetupSuccessfulStart();
        fixture.Model
            .Setup(item => item.StreamAsync(
                It.IsAny<AiModelClientOptions>(),
                It.IsAny<AiModelRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(FailedModelEvents(new AiModelClientException("ai_timeout")));

        var events = await CollectAsync(fixture.CreateService().StreamAsync(
            new AiChatRequestDto { ConversationId = 5, Prompt = "hello" },
            CancellationToken.None));

        Assert.AreEqual(AiMessageStatus.Failed, fixture.AssistantUpdate!.Status);
        Assert.AreEqual("ai_timeout", fixture.AssistantUpdate.ErrorType);
        Assert.IsTrue(events.Any(item => item.Type == "error"));
        fixture.Usage.Verify(item => item.CompleteAsync(
            301,
            AiUsageStatus.Failed,
            0,
            0,
            It.IsAny<long>(),
            "ai_timeout"), Times.Once);
    }

    [TestMethod]
    public async Task StreamAsync_RejectsMoreThanEightToolCalls()
    {
        var fixture = new ChatFixture();
        fixture.SetupSuccessfulStart();
        fixture.Model
            .Setup(item => item.StreamAsync(
                It.IsAny<AiModelClientOptions>(),
                It.IsAny<AiModelRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(ModelEvents(
                new AiModelToolCallsCompleted(
                    Enumerable.Range(1, 9)
                        .Select(index => new AiModelToolCall(
                            $"call-{index}",
                            "search_my_notes",
                            """{"keyword":"AI"}"""))
                        .ToList()),
                new AiModelCompleted()));

        var events = await CollectAsync(fixture.CreateService().StreamAsync(
            new AiChatRequestDto { ConversationId = 5, Prompt = "hello" },
            CancellationToken.None));

        Assert.AreEqual("ai_tool_limit", fixture.AssistantUpdate!.ErrorType);
        Assert.IsTrue(events.Any(item => item.Type == "error"));
        fixture.Tools.Verify(
            item => item.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        fixture.Usage.Verify(item => item.CompleteAsync(
            301,
            AiUsageStatus.Failed,
            0,
            0,
            It.IsAny<long>(),
            "ai_tool_limit"), Times.Once);
    }

    [TestMethod]
    public async Task StreamAsync_CancellationFinalizesMessageAndUsageOnce()
    {
        var fixture = new ChatFixture();
        fixture.SetupSuccessfulStart();
        fixture.Model
            .Setup(item => item.StreamAsync(
                It.IsAny<AiModelClientOptions>(),
                It.IsAny<AiModelRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns((AiModelClientOptions _, AiModelRequest _, CancellationToken token) =>
                CancelledModelEvents(token));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var events = await CollectAsync(fixture.CreateService().StreamAsync(
            new AiChatRequestDto { ConversationId = 5, Prompt = "hello" },
            cancellation.Token));

        Assert.AreEqual(AiMessageStatus.Cancelled, fixture.AssistantUpdate!.Status);
        Assert.AreEqual("ai_cancelled", fixture.AssistantUpdate.ErrorType);
        Assert.IsTrue(events.Any(item => item.Type == "error"));
        fixture.Usage.Verify(item => item.CompleteAsync(
            301,
            AiUsageStatus.Cancelled,
            0,
            0,
            It.IsAny<long>(),
            "ai_cancelled"), Times.Once);
    }

    [TestMethod]
    public async Task StreamAsync_CreateDraftToolEmitsUnsavedDraftEvent()
    {
        var fixture = new ChatFixture();
        fixture.SetupSuccessfulStart();
        fixture.Drafts
            .Setup(item => item.CreateFromModelAsync(
                5,
                102,
                AiDraftType.Note,
                """{"title":"Plan","contentMarkdown":"Body"}"""))
            .ReturnsAsync(new AiDraftEntity
            {
                Id = 601,
                ConversationId = 5,
                MessageId = 102,
                DraftType = AiDraftType.Note,
                ContentJson = """{"title":"Plan","contentMarkdown":"Body"}""",
                ExpiresAt = DateTime.Now.AddHours(1),
                Status = AiDraftStatus.Pending
            });
        fixture.Model
            .SetupSequence(item => item.StreamAsync(
                It.IsAny<AiModelClientOptions>(),
                It.IsAny<AiModelRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(ModelEvents(
                new AiModelToolCallsCompleted([
                    new AiModelToolCall(
                        "draft-1",
                        "create_draft",
                        """{"type":"note","content":{"title":"Plan","contentMarkdown":"Body"}}""")
                ]),
                new AiModelCompleted()))
            .Returns(ModelEvents(
                new AiModelTextDelta("草稿已生成"),
                new AiModelCompleted()));

        var events = await CollectAsync(fixture.CreateService().StreamAsync(
            new AiChatRequestDto { ConversationId = 5, Prompt = "create a note" },
            CancellationToken.None));

        Assert.IsTrue(events.Any(item => item.Type == "draft"));
        Assert.IsTrue(fixture.AssistantUpdate!.Content.Contains("未保存"));
        fixture.Drafts.Verify(item => item.CreateFromModelAsync(
            5,
            102,
            AiDraftType.Note,
            """{"title":"Plan","contentMarkdown":"Body"}"""), Times.Once);
    }

    [TestMethod]
    public async Task StreamAsync_YieldsMessageStartedBeforeProviderCompletes()
    {
        var fixture = new ChatFixture();
        fixture.SetupSuccessfulStart();
        var releaseProvider = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        fixture.Model
            .Setup(item => item.StreamAsync(
                It.IsAny<AiModelClientOptions>(),
                It.IsAny<AiModelRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(DelayedModelEvents(releaseProvider.Task));

        await using var enumerator = fixture.CreateService().StreamAsync(
            new AiChatRequestDto { ConversationId = 5, Prompt = "hello" },
            CancellationToken.None).GetAsyncEnumerator();
        var moveNext = enumerator.MoveNextAsync().AsTask();
        var completed = await Task.WhenAny(moveNext, Task.Delay(500));
        var completedBeforeRelease = ReferenceEquals(moveNext, completed);
        releaseProvider.TrySetResult();

        Assert.IsTrue(await moveNext);
        Assert.IsTrue(completedBeforeRelease, "message_started 应在模型生成完成前返回");
        Assert.AreEqual("message_started", enumerator.Current.Type);
    }

    [TestMethod]
    public async Task StreamAsync_IncludesConversationSummaryTokensInUsage()
    {
        var fixture = new ChatFixture();
        fixture.SetupSuccessfulStart();
        fixture.Messages
            .Setup(item => item.PageQueryAsync(
                It.IsAny<Expression<Func<AiMessageEntity, bool>>>(),
                It.IsAny<OrderByCondition>(),
                1,
                100,
                It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new PageQueryResult<AiMessageEntity>
            {
                List =
                [
                    new AiMessageEntity
                    {
                        Id = 90,
                        TenantId = 7,
                        UserId = 11,
                        ConversationId = 5,
                        Sequence = 1,
                        Role = AiMessageRole.User,
                        Status = AiMessageStatus.Completed,
                        Content = new string('旧', 13000)
                    }
                ]
            });
        fixture.Model
            .SetupSequence(item => item.StreamAsync(
                It.IsAny<AiModelClientOptions>(),
                It.IsAny<AiModelRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(ModelEvents(
                new AiModelTextDelta("旧会话摘要"),
                new AiModelUsageCompleted(3, 2, 5),
                new AiModelCompleted()))
            .Returns(ModelEvents(
                new AiModelTextDelta("回答"),
                new AiModelUsageCompleted(5, 4, 9),
                new AiModelCompleted()));

        await CollectAsync(fixture.CreateService().StreamAsync(
            new AiChatRequestDto { ConversationId = 5, Prompt = "继续" },
            CancellationToken.None));

        fixture.Usage.Verify(item => item.CompleteAsync(
            301,
            AiUsageStatus.Succeeded,
            8,
            6,
            It.IsAny<long>(),
            null), Times.Once);
    }

    private static async Task<List<AiStreamEventDto>> CollectAsync(
        IAsyncEnumerable<AiStreamEventDto> events)
    {
        var result = new List<AiStreamEventDto>();
        await foreach (var item in events)
        {
            result.Add(item);
        }
        return result;
    }

    private static async IAsyncEnumerable<AiModelStreamEvent> ModelEvents(
        params AiModelStreamEvent[] events)
    {
        foreach (var item in events)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<AiModelStreamEvent> FailedModelEvents(
        Exception exception)
    {
        await Task.Yield();
        throw exception;
#pragma warning disable CS0162
        yield break;
#pragma warning restore CS0162
    }

    private static async IAsyncEnumerable<AiModelStreamEvent> CancelledModelEvents(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        yield return new AiModelCompleted();
    }

    private static async IAsyncEnumerable<AiModelStreamEvent> DelayedModelEvents(Task release)
    {
        await release;
        yield return new AiModelTextDelta("done");
        yield return new AiModelCompleted();
    }

    private sealed class ChatFixture
    {
        public Mock<IAiConversationService> Conversations { get; } = new();
        public Mock<IAiAdminService> Admin { get; } = new();
        public Mock<IAiGenerationCoordinator> Coordinator { get; } = new();
        public Mock<IAiQuotaCounter> Quota { get; } = new();
        public Mock<IAiUsageRecorder> Usage { get; } = new();
        public Mock<IAiToolService> Tools { get; } = new();
        public Mock<IAiDraftService> Drafts { get; } = new();
        public Mock<IAiModelClient> Model { get; } = new();
        public Mock<IAiConversationRepository> ConversationRepository { get; } = new();
        public Mock<IAiMessageRepository> Messages { get; } = new();
        public Mock<IAiSourceRepository> Sources { get; } = new();

        public AiMessageEntity? AssistantUpdate { get; private set; }
        public List<AiSourceEntity> AddedSources { get; } = [];

        public ChatFixture()
        {
            Coordinator
                .Setup(item => item.TryAcquireAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync("lease-token");
            Coordinator
                .Setup(item => item.IsCancellationRequestedAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()))
                .ReturnsAsync(false);
            Coordinator
                .Setup(item => item.ReleaseAsync(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
        }

        public void SetupConversation()
        {
            Conversations
                .Setup(item => item.GetOwnedAsync(5))
                .ReturnsAsync(new AiConversationEntity
                {
                    Id = 5,
                    TenantId = 7,
                    UserId = 11,
                    Title = "新会话"
                });
        }

        public void SetupMessagePersistence()
        {
            Messages
                .Setup(item => item.PageQueryAsync(
                    It.IsAny<Expression<Func<AiMessageEntity, bool>>>(),
                    It.IsAny<OrderByCondition>(),
                    1,
                    It.IsAny<int>(),
                    It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new PageQueryResult<AiMessageEntity> { List = [] });
            var transaction = Mock.Of<IDbTransaction>();
            ConversationRepository
                .Setup(item => item.ExecuteAutoTransactionAsync(
                    It.IsAny<Func<IDbTransaction, Task<bool>>>(),
                    null,
                    null))
                .Returns((Func<IDbTransaction, Task<bool>> callback, IDbTransaction? _, IDbConnection? __) =>
                    callback(transaction));
            Messages
                .Setup(item => item.AddAsync(
                    It.IsAny<AiMessageEntity>(),
                    false,
                    null,
                    transaction))
                .Callback<AiMessageEntity, bool, Expression<Func<AiMessageEntity, object>>?, IDbTransaction?>(
                    (entity, _, _, _) => entity.Id = entity.Role == AiMessageRole.User ? 101 : 102)
                .ReturnsAsync(true);
            Messages
                .Setup(item => item.UpdateAsync(
                    It.IsAny<AiMessageEntity>(),
                    It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
                    It.IsAny<Expression<Func<AiMessageEntity, bool>>>(),
                    It.IsAny<IDbTransaction>()))
                .Callback<AiMessageEntity, Expression<Func<AiMessageEntity, object>>?, Expression<Func<AiMessageEntity, bool>>?, IDbTransaction?>(
                    (entity, _, _, _) => AssistantUpdate = entity)
                .ReturnsAsync(1);
            Sources
                .Setup(item => item.AddAsync(
                    It.IsAny<AiSourceEntity>(),
                    false,
                    null,
                    It.IsAny<IDbTransaction>()))
                .Callback<AiSourceEntity, bool, Expression<Func<AiSourceEntity, object>>?, IDbTransaction?>(
                    (entity, _, _, _) => AddedSources.Add(entity))
                .ReturnsAsync(true);
        }

        public void SetupSuccessfulStart()
        {
            SetupConversation();
            SetupMessagePersistence();
            Admin
                .Setup(item => item.GetRuntimeConfigAsync(7))
                .ReturnsAsync(new AiRuntimeConfig
                {
                    BaseUrl = "https://api.example.com/v1",
                    ApiKey = "sk-test",
                    Model = "test-model",
                    TimeoutSeconds = 30,
                    MaxOutputTokens = 1024,
                    Temperature = 0.2m,
                    DailyRequestLimit = 5
                });
            Quota
                .Setup(item => item.ReserveAsync(7, 5, It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);
            Usage
                .Setup(item => item.StartAsync(5, 102, "test-model"))
                .ReturnsAsync(new AiUsageEntity { Id = 301 });
            Tools.Setup(item => item.GetDefinitions()).Returns([]);
        }

        public AiChatService CreateService()
        {
            return new AiChatService(
                Conversations.Object,
                Admin.Object,
                Coordinator.Object,
                Quota.Object,
                Usage.Object,
                Tools.Object,
                Drafts.Object,
                Model.Object,
                ConversationRepository.Object,
                Messages.Object,
                Sources.Object,
                NullLogger<AiChatService>.Instance);
        }
    }
}
