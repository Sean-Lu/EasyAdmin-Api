using System.Data;
using System.Linq.Expressions;
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
public class AiConversationServiceTests
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
    public async Task CreateAsync_AssignsCurrentOwnerAndDefaultTitle()
    {
        var repositories = new RepositoryMocks();
        AiConversationEntity? added = null;
        repositories.Conversations
            .Setup(repository => repository.AddAsync(
                It.IsAny<AiConversationEntity>(),
                false,
                null,
                null))
            .Callback<AiConversationEntity, bool, Expression<Func<AiConversationEntity, object>>?, IDbTransaction?>(
                (entity, _, _, _) =>
                {
                    entity.Id = 101;
                    added = entity;
                })
            .ReturnsAsync(true);

        var result = await repositories.CreateService().CreateAsync();

        Assert.IsNotNull(added);
        Assert.AreEqual(7, added.TenantId);
        Assert.AreEqual(11, added.UserId);
        Assert.AreEqual("新会话", added.Title);
        Assert.AreEqual(101, result.Id);
    }

    [TestMethod]
    public async Task GetOwnedAsync_UsesTenantUserAndActivePredicates()
    {
        var repositories = new RepositoryMocks();
        Expression<Func<AiConversationEntity, bool>>? predicate = null;
        repositories.Conversations
            .Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<AiConversationEntity, bool>>>(),
                It.IsAny<Expression<Func<AiConversationEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<AiConversationEntity, bool>>, Expression<Func<AiConversationEntity, object>>?, bool>(
                (value, _, _) => predicate = value)
            .ReturnsAsync(new AiConversationEntity { Id = 5, TenantId = 7, UserId = 11, Title = "会话" });

        await repositories.CreateService().GetOwnedAsync(5);

        AssertOwnerPredicate(predicate!, 5);
    }

    [TestMethod]
    public async Task PageAsync_UsesCurrentOwnerPredicate()
    {
        var repositories = new RepositoryMocks();
        Expression<Func<AiConversationEntity, bool>>? predicate = null;
        repositories.Conversations
            .Setup(repository => repository.PageQueryAsync(
                It.IsAny<Expression<Func<AiConversationEntity, bool>>>(),
                It.IsAny<Sean.Core.DbRepository.OrderByCondition>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<AiConversationEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<AiConversationEntity, bool>>, Sean.Core.DbRepository.OrderByCondition, int, int, Expression<Func<AiConversationEntity, object>>?, bool>(
                (value, _, _, _, _, _) => predicate = value)
            .ReturnsAsync(new Sean.Core.DbRepository.PageQueryResult<AiConversationEntity>
            {
                Total = 1,
                List = new List<AiConversationEntity>
                {
                    Owned(new AiConversationEntity { Id = 5, Title = "会话" })
                }
            });

        var result = await repositories.CreateService().PageAsync(new AiConversationPageReqDto());

        Assert.IsTrue(predicate!.Compile()(Owned(new AiConversationEntity { Id = 5, Title = "会话" })));
        Assert.IsTrue(RejectsOtherOwner(predicate, new AiConversationEntity { Id = 5, Title = "会话" }));
        Assert.AreEqual(1, result.Total);
        Assert.AreEqual(5, result.List[0].Id);
    }

    [TestMethod]
    public async Task RenameAsync_TrimsTitleAndRejectsInvalidLength()
    {
        var repositories = new RepositoryMocks();
        repositories.SetupOwnedConversation();
        AiConversationEntity? update = null;
        Expression<Func<AiConversationEntity, bool>>? predicate = null;
        repositories.Conversations
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<AiConversationEntity>(),
                It.IsAny<Expression<Func<AiConversationEntity, object>>>(),
                It.IsAny<Expression<Func<AiConversationEntity, bool>>>(),
                null))
            .Callback<AiConversationEntity, Expression<Func<AiConversationEntity, object>>?, Expression<Func<AiConversationEntity, bool>>?, IDbTransaction?>(
                (entity, _, value, _) =>
                {
                    update = entity;
                    predicate = value;
                })
            .ReturnsAsync(1);

        var result = await repositories.CreateService().RenameAsync(5, "  新标题  ");

        Assert.AreEqual("新标题", update?.Title);
        Assert.AreEqual("新标题", result.Title);
        AssertOwnerPredicate(predicate!, 5);
        await Assert.ThrowsAsync<ExplicitException>(() => repositories.CreateService().RenameAsync(5, " "));
        await Assert.ThrowsAsync<ExplicitException>(() => repositories.CreateService().RenameAsync(5, new string('x', 201)));
    }

    [TestMethod]
    public async Task DeleteAsync_SoftDeletesOwnedGraphInOneTransaction()
    {
        var repositories = new RepositoryMocks();
        repositories.SetupOwnedConversation();
        var transaction = Mock.Of<IDbTransaction>();
        repositories.Conversations
            .Setup(repository => repository.ExecuteAutoTransactionAsync(
                It.IsAny<Func<IDbTransaction, Task<bool>>>(),
                null,
                null))
            .Returns((Func<IDbTransaction, Task<bool>> callback, IDbTransaction? _, IDbConnection? __) =>
                callback(transaction));

        Expression<Func<AiConversationEntity, bool>>? conversationPredicate = null;
        Expression<Func<AiMessageEntity, bool>>? messagePredicate = null;
        Expression<Func<AiSourceEntity, bool>>? sourcePredicate = null;
        Expression<Func<AiDraftEntity, bool>>? draftPredicate = null;
        AiDraftEntity? draftUpdate = null;

        repositories.Conversations
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<AiConversationEntity>(),
                It.IsAny<Expression<Func<AiConversationEntity, object>>>(),
                It.IsAny<Expression<Func<AiConversationEntity, bool>>>(),
                transaction))
            .Callback<AiConversationEntity, Expression<Func<AiConversationEntity, object>>?, Expression<Func<AiConversationEntity, bool>>?, IDbTransaction?>(
                (_, _, predicate, _) => conversationPredicate = predicate)
            .ReturnsAsync(1);
        repositories.Messages
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<AiMessageEntity>(),
                It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
                It.IsAny<Expression<Func<AiMessageEntity, bool>>>(),
                transaction))
            .Callback<AiMessageEntity, Expression<Func<AiMessageEntity, object>>?, Expression<Func<AiMessageEntity, bool>>?, IDbTransaction?>(
                (_, _, predicate, _) => messagePredicate = predicate)
            .ReturnsAsync(1);
        repositories.Sources
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<AiSourceEntity>(),
                It.IsAny<Expression<Func<AiSourceEntity, object>>>(),
                It.IsAny<Expression<Func<AiSourceEntity, bool>>>(),
                transaction))
            .Callback<AiSourceEntity, Expression<Func<AiSourceEntity, object>>?, Expression<Func<AiSourceEntity, bool>>?, IDbTransaction?>(
                (_, _, predicate, _) => sourcePredicate = predicate)
            .ReturnsAsync(1);
        repositories.Drafts
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<AiDraftEntity>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                transaction))
            .Callback<AiDraftEntity, Expression<Func<AiDraftEntity, object>>?, Expression<Func<AiDraftEntity, bool>>?, IDbTransaction?>(
                (entity, _, predicate, _) =>
                {
                    draftUpdate = entity;
                    draftPredicate = predicate;
                })
            .ReturnsAsync(1);

        await repositories.CreateService().DeleteAsync(5);

        Assert.IsTrue(RejectsOtherOwner(conversationPredicate!, new AiConversationEntity { Id = 5 }));
        Assert.IsTrue(RejectsOtherOwner(messagePredicate!, new AiMessageEntity { ConversationId = 5 }));
        Assert.IsTrue(RejectsOtherOwner(sourcePredicate!, new AiSourceEntity { ConversationId = 5 }));
        Assert.IsTrue(RejectsOtherOwner(draftPredicate!, new AiDraftEntity { ConversationId = 5, Status = AiDraftStatus.Pending }));
        Assert.IsFalse(draftPredicate!.Compile()(Owned(new AiDraftEntity { ConversationId = 5, Status = AiDraftStatus.Confirmed })));
        Assert.IsTrue(draftUpdate?.IsDelete);
        Assert.AreEqual(AiDraftStatus.Deleted, draftUpdate?.Status);

        repositories.Conversations.Verify(repository => repository.UpdateAsync(
            It.IsAny<AiConversationEntity>(),
            It.IsAny<Expression<Func<AiConversationEntity, object>>>(),
            It.IsAny<Expression<Func<AiConversationEntity, bool>>>(),
            transaction), Times.Once);
        repositories.Messages.Verify(repository => repository.UpdateAsync(
            It.IsAny<AiMessageEntity>(),
            It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
            It.IsAny<Expression<Func<AiMessageEntity, bool>>>(),
            transaction), Times.Once);
        repositories.Sources.Verify(repository => repository.UpdateAsync(
            It.IsAny<AiSourceEntity>(),
            It.IsAny<Expression<Func<AiSourceEntity, object>>>(),
            It.IsAny<Expression<Func<AiSourceEntity, bool>>>(),
            transaction), Times.Once);
        repositories.Drafts.Verify(repository => repository.UpdateAsync(
            It.IsAny<AiDraftEntity>(),
            It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
            It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
            transaction), Times.Once);
    }

    [TestMethod]
    public async Task PageMessagesAsync_ScopesMessagesSourcesAndDraftsToOwnedConversation()
    {
        var repositories = new RepositoryMocks();
        repositories.SetupOwnedConversation();
        Expression<Func<AiMessageEntity, bool>>? messagePredicate = null;
        Expression<Func<AiSourceEntity, bool>>? sourcePredicate = null;
        Expression<Func<AiDraftEntity, bool>>? draftPredicate = null;
        repositories.Messages
            .Setup(repository => repository.PageQueryAsync(
                It.IsAny<Expression<Func<AiMessageEntity, bool>>>(),
                It.IsAny<Sean.Core.DbRepository.OrderByCondition>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<AiMessageEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<AiMessageEntity, bool>>, Sean.Core.DbRepository.OrderByCondition, int, int, Expression<Func<AiMessageEntity, object>>?, bool>(
                (predicate, _, _, _, _, _) => messagePredicate = predicate)
            .ReturnsAsync(new Sean.Core.DbRepository.PageQueryResult<AiMessageEntity>
            {
                Total = 2,
                List = new List<AiMessageEntity>
                {
                    Owned(new AiMessageEntity { Id = 32, ConversationId = 5, Sequence = 2, Content = "新回答" }),
                    Owned(new AiMessageEntity { Id = 31, ConversationId = 5, Sequence = 1, Content = "回答" })
                }
            });
        repositories.Sources
            .Setup(repository => repository.QueryAsync(
                It.IsAny<Expression<Func<AiSourceEntity, bool>>>(),
                It.IsAny<Sean.Core.DbRepository.OrderByCondition>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Expression<Func<AiSourceEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<AiSourceEntity, bool>>, Sean.Core.DbRepository.OrderByCondition?, int?, int?, Expression<Func<AiSourceEntity, object>>?, bool>(
                (predicate, _, _, _, _, _) => sourcePredicate = predicate)
            .ReturnsAsync(new List<AiSourceEntity>
            {
                Owned(new AiSourceEntity
                {
                    Id = 41,
                    ConversationId = 5,
                    MessageId = 31,
                    Title = "来源",
                    SourceDate = new DateTime(2026, 7, 27, 9, 30, 0),
                    Route = "/custom/source/41"
                })
            });
        repositories.Drafts
            .Setup(repository => repository.QueryAsync(
                It.IsAny<Expression<Func<AiDraftEntity, bool>>>(),
                It.IsAny<Sean.Core.DbRepository.OrderByCondition>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Expression<Func<AiDraftEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<AiDraftEntity, bool>>, Sean.Core.DbRepository.OrderByCondition?, int?, int?, Expression<Func<AiDraftEntity, object>>?, bool>(
                (predicate, _, _, _, _, _) => draftPredicate = predicate)
            .ReturnsAsync(new List<AiDraftEntity>
            {
                Owned(new AiDraftEntity
                {
                    Id = 51,
                    ConversationId = 5,
                    MessageId = 31,
                    DraftType = AiDraftType.Note,
                    ContentJson = """{"title":"草稿","contentMarkdown":"内容"}""",
                    Status = AiDraftStatus.Pending
                })
            });

        var result = await repositories.CreateService().PageMessagesAsync(new AiMessagePageReqDto
        {
            ConversationId = 5,
            PageNumber = 1,
            PageSize = 10
        });

        Assert.IsTrue(RejectsOtherOwner(messagePredicate!, new AiMessageEntity { ConversationId = 5 }));
        Assert.IsTrue(RejectsOtherOwner(sourcePredicate!, new AiSourceEntity { ConversationId = 5, MessageId = 31 }));
        Assert.IsFalse(sourcePredicate!.Compile()(Owned(new AiSourceEntity { ConversationId = 5, MessageId = 99 })));
        Assert.IsTrue(RejectsOtherOwner(draftPredicate!, new AiDraftEntity { ConversationId = 5, MessageId = 31 }));
        Assert.IsFalse(draftPredicate!.Compile()(Owned(new AiDraftEntity { ConversationId = 5, MessageId = 99 })));
        Assert.AreEqual(2, result.List.Count);
        CollectionAssert.AreEqual(new long[] { 31, 32 }, result.List.Select(item => item.Id).ToArray());
        Assert.AreEqual(1, result.List[0].Sources.Count);
        Assert.AreEqual(41, result.List[0].Sources[0].Id);
        Assert.AreEqual(new DateTime(2026, 7, 27, 9, 30, 0), result.List[0].Sources[0].Date);
        Assert.AreEqual("/custom/source/41", result.List[0].Sources[0].Route);
        Assert.AreEqual(1, result.List[0].Drafts.Count);
        Assert.AreEqual(51, result.List[0].Drafts[0].Id);
    }

    private static void AssertOwnerPredicate(Expression<Func<AiConversationEntity, bool>> predicate, long id)
    {
        var compiled = predicate.Compile();
        Assert.IsTrue(compiled(Owned(new AiConversationEntity { Id = id })));
        Assert.IsFalse(compiled(new AiConversationEntity { Id = id, TenantId = 8, UserId = 11 }));
        Assert.IsFalse(compiled(new AiConversationEntity { Id = id, TenantId = 7, UserId = 12 }));
        Assert.IsFalse(compiled(new AiConversationEntity { Id = id, TenantId = 7, UserId = 11, IsDelete = true }));
        Assert.IsFalse(compiled(Owned(new AiConversationEntity { Id = id + 1 })));
    }

    private static bool RejectsOtherOwner<TEntity>(Expression<Func<TEntity, bool>> predicate, TEntity entity)
        where TEntity : TenantEntityBase
    {
        var compiled = predicate.Compile();
        entity.TenantId = 8;
        SetUserId(entity, 11);
        var rejectsTenant = !compiled(entity);
        entity.TenantId = 7;
        SetUserId(entity, 12);
        var rejectsUser = !compiled(entity);
        SetUserId(entity, 11);
        entity.IsDelete = true;
        var rejectsDeleted = !compiled(entity);
        return rejectsTenant && rejectsUser && rejectsDeleted;
    }

    private static TEntity Owned<TEntity>(TEntity entity) where TEntity : TenantEntityBase
    {
        entity.TenantId = 7;
        SetUserId(entity, 11);
        return entity;
    }

    private static void SetUserId<TEntity>(TEntity entity, long userId)
    {
        typeof(TEntity).GetProperty("UserId")!.SetValue(entity, userId);
    }

    private sealed class RepositoryMocks
    {
        public Mock<IAiConversationRepository> Conversations { get; } = new();
        public Mock<IAiMessageRepository> Messages { get; } = new();
        public Mock<IAiSourceRepository> Sources { get; } = new();
        public Mock<IAiDraftRepository> Drafts { get; } = new();
        public AiConversationService CreateService()
        {
            return new AiConversationService(
                Conversations.Object,
                Messages.Object,
                Sources.Object,
                Drafts.Object);
        }

        public void SetupOwnedConversation()
        {
            Conversations
                .Setup(repository => repository.GetAsync(
                    It.IsAny<Expression<Func<AiConversationEntity, bool>>>(),
                    It.IsAny<Expression<Func<AiConversationEntity, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new AiConversationEntity { Id = 5, TenantId = 7, UserId = 11, Title = "会话" });
        }
    }
}
