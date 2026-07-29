using System.Data;
using System.Linq.Expressions;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Ai;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EasyAdmin.Test;

[TestClass]
public class AiAdminServiceTests
{
    [TestMethod]
    public async Task GetModelConfigAsync_MasksStoredApiKey()
    {
        var protector = CreateProtector();
        var repositories = CreateRepositories();
        repositories.Model.SetupGet(new AiModelConfigEntity
        {
            BaseUrl = "https://api.example.com/v1",
            Model = "test-model",
            ApiKeyEncrypted = protector.Encrypt("sk-very-secret"),
            TimeoutSeconds = 30,
            MaxOutputTokens = 2048,
            Temperature = 0.5m
        });

        var result = await CreateService(repositories, protector).GetModelConfigAsync();

        Assert.IsTrue(result.HasApiKey);
        Assert.AreEqual("sk-****cret", result.MaskedApiKey);
        Assert.IsFalse(result.GetType().GetProperties().Any(property =>
            property.Name.Contains("Encrypted", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task UpdateModelConfigAsync_BlankApiKeyPreservesCiphertext()
    {
        var protector = CreateProtector();
        var repositories = CreateRepositories();
        var ciphertext = protector.Encrypt("sk-existing");
        var entity = new AiModelConfigEntity { ApiKeyEncrypted = ciphertext };
        repositories.Model.SetupGet(entity);
        repositories.Model.CaptureUpdate();

        await CreateService(repositories, protector).UpdateModelConfigAsync(new AiModelConfigUpdateDto
        {
            BaseUrl = "https://api.example.com/v1",
            Model = "test-model",
            ApiKey = " ",
            TimeoutSeconds = 30,
            MaxOutputTokens = 2048,
            Temperature = 0.5m
        });

        Assert.AreEqual(ciphertext, entity.ApiKeyEncrypted);
    }

    [TestMethod]
    public async Task UpdateModelConfigAsync_MissingNewApiKeyThrowsExplicitException()
    {
        var repositories = CreateRepositories();

        await Assert.ThrowsAsync<ExplicitException>(() =>
            CreateService(repositories, CreateProtector()).UpdateModelConfigAsync(new AiModelConfigUpdateDto
            {
                BaseUrl = "https://api.example.com/v1",
                Model = "test-model",
                TimeoutSeconds = 30,
                MaxOutputTokens = 2048,
                Temperature = 0.5m
            }));
    }

    [TestMethod]
    public async Task UpdateModelConfigAsync_NonHttpsRemoteBaseUrlThrowsExplicitException()
    {
        var repositories = CreateRepositories();

        await Assert.ThrowsAsync<ExplicitException>(() =>
            CreateService(repositories, CreateProtector()).UpdateModelConfigAsync(new AiModelConfigUpdateDto
            {
                BaseUrl = "http://api.example.com/v1",
                Model = "test-model",
                ApiKey = "sk-new",
                TimeoutSeconds = 30,
                MaxOutputTokens = 2048,
                Temperature = 0.5m
            }));
    }

    [TestMethod]
    public async Task GetTenantSettingAsync_MissingSettingDefaultsToDisabled()
    {
        var repositories = CreateRepositories();

        var result = await CreateService(repositories, CreateProtector()).GetTenantSettingAsync(7);

        Assert.AreEqual(7, result.TenantId);
        Assert.IsFalse(result.Enabled);
        Assert.AreEqual(0, result.DailyRequestLimit);
    }

    [TestMethod]
    public async Task UpdateTenantSettingAsync_EnabledWithNonpositiveLimitThrowsExplicitException()
    {
        var repositories = CreateRepositories();

        await Assert.ThrowsAsync<ExplicitException>(() =>
            CreateService(repositories, CreateProtector()).UpdateTenantSettingAsync(new AiTenantSettingUpdateDto
            {
                TenantId = 7,
                Enabled = true,
                DailyRequestLimit = 0
            }));
    }

    [TestMethod]
    public async Task GetRuntimeConfigAsync_DisabledTenantThrowsExplicitException()
    {
        var repositories = CreateRepositories();

        await Assert.ThrowsAsync<ExplicitException>(() =>
            CreateService(repositories, CreateProtector()).GetRuntimeConfigAsync(7));
    }

    [TestMethod]
    public async Task TestConnectionAsync_UsesMinimalRequestAndRecordsSystemUsageWithoutQuota()
    {
        var protector = CreateProtector();
        var repositories = CreateRepositories();
        repositories.Model.SetupGet(new AiModelConfigEntity
        {
            BaseUrl = "https://api.example.com/v1",
            Model = "test-model",
            ApiKeyEncrypted = protector.Encrypt("sk-secret"),
            TimeoutSeconds = 30,
            MaxOutputTokens = 2048,
            Temperature = 0.5m
        });
        AiModelClientOptions? capturedOptions = null;
        AiModelRequest? capturedRequest = null;
        var modelClient = new Mock<IAiModelClient>();
        modelClient
            .Setup(item => item.StreamAsync(
                It.IsAny<AiModelClientOptions>(),
                It.IsAny<AiModelRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<AiModelClientOptions, AiModelRequest, CancellationToken>((options, request, _) =>
            {
                capturedOptions = options;
                capturedRequest = request;
            })
            .Returns(ConnectionEvents());
        var usageRecorder = new Mock<IAiUsageRecorder>();
        usageRecorder
            .Setup(item => item.StartAsync(0, 0, "test-model"))
            .ReturnsAsync(new AiUsageEntity { Id = 901 });

        var service = new AiAdminService(
            repositories.Model.Object,
            repositories.TenantSetting.Object,
            repositories.Usage.Object,
            repositories.Tenant.Object,
            repositories.Users.Object,
            protector,
            modelClient.Object,
            usageRecorder.Object);
        var result = await service.TestConnectionAsync();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("OK", result.Result);
        Assert.IsTrue(result.LatencyMs >= 0);
        Assert.AreEqual("https://api.example.com/v1", capturedOptions!.BaseUrl);
        Assert.AreEqual("sk-secret", capturedOptions.ApiKey);
        Assert.AreEqual("test-model", capturedOptions.Model);
        Assert.AreEqual(16, capturedRequest!.MaxOutputTokens);
        Assert.AreEqual(0, capturedRequest.Tools.Count);
        Assert.AreEqual(1, capturedRequest.Messages.Count);
        Assert.AreEqual("user", capturedRequest.Messages[0].Role);
        Assert.AreEqual("只回复 OK", capturedRequest.Messages[0].Content);
        usageRecorder.Verify(item => item.CompleteAsync(
            901,
            AiUsageStatus.Succeeded,
            2,
            1,
            It.IsAny<long>(),
            null), Times.Once);
    }

    [TestMethod]
    public async Task TestConnectionAsync_InvalidEncryptedKeyFinalizesUsageBeforeRethrowing()
    {
        var repositories = CreateRepositories();
        repositories.Model.SetupGet(new AiModelConfigEntity
        {
            BaseUrl = "https://api.example.com/v1",
            Model = "test-model",
            ApiKeyEncrypted = "invalid-ciphertext",
            TimeoutSeconds = 30,
            MaxOutputTokens = 2048,
            Temperature = 0.5m
        });
        var usageRecorder = new Mock<IAiUsageRecorder>();
        usageRecorder
            .Setup(item => item.StartAsync(0, 0, "test-model"))
            .ReturnsAsync(new AiUsageEntity { Id = 902 });
        var service = new AiAdminService(
            repositories.Model.Object,
            repositories.TenantSetting.Object,
            repositories.Usage.Object,
            repositories.Tenant.Object,
            Mock.Of<IUserRepository>(),
            CreateProtector(),
            Mock.Of<IAiModelClient>(),
            usageRecorder.Object);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.TestConnectionAsync());

        usageRecorder.Verify(item => item.CompleteAsync(
            902,
            AiUsageStatus.Failed,
            0,
            0,
            It.IsAny<long>(),
            "ai_business_error"), Times.Once);
    }

    [TestMethod]
    public async Task PageUsageAsync_UserKeywordMatchesUserNameOrNickname()
    {
        var repositories = CreateRepositories();
        repositories.Users
            .Setup(item => item.QueryAsync(
                It.IsAny<Expression<Func<UserEntity, bool>>>(),
                It.IsAny<Sean.Core.DbRepository.OrderByCondition>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Expression<Func<UserEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync([
                new UserEntity { Id = 21, TenantId = 7, UserName = "sean", NickName = "肖恩" }
            ]);
        Expression<Func<AiUsageEntity, bool>>? predicate = null;
        repositories.Usage
            .Setup(item => item.PageQueryAsync(
                It.IsAny<Expression<Func<AiUsageEntity, bool>>>(),
                It.IsAny<Sean.Core.DbRepository.OrderByCondition>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<AiUsageEntity, object>>>(),
                It.IsAny<bool>()))
            .Callback<Expression<Func<AiUsageEntity, bool>>, Sean.Core.DbRepository.OrderByCondition, int, int, Expression<Func<AiUsageEntity, object>>?, bool>(
                (value, _, _, _, _, _) => predicate = value)
            .ReturnsAsync(new Sean.Core.DbRepository.PageQueryResult<AiUsageEntity>
            {
                List = []
            });

        await CreateService(repositories, CreateProtector()).PageUsageAsync(new AiUsagePageReqDto
        {
            TenantId = 7,
            UserKeyword = " sean ",
            PageNumber = 1,
            PageSize = 20
        });

        Assert.IsTrue(predicate!.Compile()(new AiUsageEntity { TenantId = 7, UserId = 21 }));
        Assert.IsFalse(predicate.Compile()(new AiUsageEntity { TenantId = 7, UserId = 22 }));
    }

    private static AiAdminService CreateService(RepositoryMocks repositories, AiApiKeyProtector protector)
    {
        return new AiAdminService(
            repositories.Model.Object,
            repositories.TenantSetting.Object,
            repositories.Usage.Object,
            repositories.Tenant.Object,
            repositories.Users.Object,
            protector,
            Mock.Of<IAiModelClient>(),
            Mock.Of<IAiUsageRecorder>());
    }

    private static RepositoryMocks CreateRepositories()
    {
        return new RepositoryMocks(
            new Mock<IAiModelConfigRepository>(),
            new Mock<IAiTenantSettingRepository>(),
            new Mock<IAiUsageRepository>(),
            new Mock<ITenantRepository>(),
            new Mock<IUserRepository>());
    }

    private static AiApiKeyProtector CreateProtector()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "unit-test-jwt-secret"
            })
            .Build();
        return new AiApiKeyProtector(configuration);
    }

    private static async IAsyncEnumerable<AiModelStreamEvent> ConnectionEvents()
    {
        yield return new AiModelTextDelta("OK");
        yield return new AiModelUsageCompleted(2, 1, 3);
        yield return new AiModelCompleted();
        await Task.CompletedTask;
    }

    private sealed record RepositoryMocks(
        Mock<IAiModelConfigRepository> Model,
        Mock<IAiTenantSettingRepository> TenantSetting,
        Mock<IAiUsageRepository> Usage,
        Mock<ITenantRepository> Tenant,
        Mock<IUserRepository> Users);
}

internal static class AiAdminRepositoryMockExtensions
{
    public static void SetupGet(this Mock<IAiModelConfigRepository> repository, AiModelConfigEntity entity)
    {
        repository.Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<AiModelConfigEntity, bool>>>(),
                It.IsAny<Expression<Func<AiModelConfigEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(entity);
    }

    public static void CaptureUpdate(this Mock<IAiModelConfigRepository> repository)
    {
        repository.Setup(item => item.UpdateAsync(
                It.IsAny<AiModelConfigEntity>(),
                It.IsAny<Expression<Func<AiModelConfigEntity, object>>>(),
                It.IsAny<Expression<Func<AiModelConfigEntity, bool>>>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(1);
    }
}

[TestClass]
public class AiAdminServiceTestsUsageRecorder
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
    public async Task StartAsync_CreatesOwnedRunningUsageWithoutPromptData()
    {
        var repository = new Mock<IAiUsageRepository>();
        AiUsageEntity? added = null;
        repository
            .Setup(item => item.AddAsync(
                It.IsAny<AiUsageEntity>(),
                false,
                null,
                null))
            .Callback<AiUsageEntity, bool, Expression<Func<AiUsageEntity, object>>?, IDbTransaction?>(
                (entity, _, _, _) =>
                {
                    entity.Id = 101;
                    added = entity;
                })
            .ReturnsAsync(true);

        var result = await new AiUsageRecorder(repository.Object).StartAsync(21, 31, " test-model ");

        Assert.AreSame(added, result);
        Assert.AreEqual(101, result.Id);
        Assert.AreEqual(7, result.TenantId);
        Assert.AreEqual(11, result.UserId);
        Assert.AreEqual(21, result.ConversationId);
        Assert.AreEqual(31, result.MessageId);
        Assert.AreEqual("test-model", result.Model);
        Assert.AreEqual(AiUsageStatus.Running, result.Status);
        Assert.IsFalse(result.GetType().GetProperties().Any(property =>
            property.Name.Contains("Prompt", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    [DataRow(AiUsageStatus.Failed)]
    [DataRow(AiUsageStatus.Cancelled)]
    public async Task CompleteAsync_FailureOrCancellationUpdatesOwnedRowAndRetainsUsage(AiUsageStatus status)
    {
        var repository = new Mock<IAiUsageRepository>();
        AiUsageEntity? update = null;
        Expression<Func<AiUsageEntity, bool>>? predicate = null;
        repository
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiUsageEntity>(),
                It.IsAny<Expression<Func<AiUsageEntity, object>>>(),
                It.IsAny<Expression<Func<AiUsageEntity, bool>>>(),
                null))
            .Callback<AiUsageEntity, Expression<Func<AiUsageEntity, object>>?, Expression<Func<AiUsageEntity, bool>>?, IDbTransaction?>(
                (entity, _, value, _) =>
                {
                    update = entity;
                    predicate = value;
                })
            .ReturnsAsync(1);

        await new AiUsageRecorder(repository.Object)
            .CompleteAsync(101, status, 12, 8, 345, " AI_TIMEOUT ");

        Assert.IsNotNull(update);
        Assert.AreEqual(status, update.Status);
        Assert.AreEqual(12, update.InputTokens);
        Assert.AreEqual(8, update.OutputTokens);
        Assert.AreEqual(20, update.TotalTokens);
        Assert.AreEqual(345, update.DurationMs);
        Assert.AreEqual("ai_timeout", update.ErrorType);
        Assert.IsFalse(update.IsDelete);

        var compiled = predicate!.Compile();
        Assert.IsTrue(compiled(new AiUsageEntity { Id = 101, TenantId = 7, UserId = 11 }));
        Assert.IsFalse(compiled(new AiUsageEntity { Id = 101, TenantId = 8, UserId = 11 }));
        Assert.IsFalse(compiled(new AiUsageEntity { Id = 101, TenantId = 7, UserId = 12 }));
        Assert.IsFalse(compiled(new AiUsageEntity { Id = 101, TenantId = 7, UserId = 11, IsDelete = true }));
        Assert.IsFalse(compiled(new AiUsageEntity
        {
            Id = 101,
            TenantId = 7,
            UserId = 11,
            Status = AiUsageStatus.Succeeded
        }));
    }

    [TestMethod]
    public async Task CompleteAsync_RejectsRunningAsFinalStatus()
    {
        var repository = new Mock<IAiUsageRepository>();

        await Assert.ThrowsAsync<ExplicitException>(() =>
            new AiUsageRecorder(repository.Object)
                .CompleteAsync(101, AiUsageStatus.Running, 0, 0, 0, null));

        repository.Verify(item => item.UpdateAsync(
            It.IsAny<AiUsageEntity>(),
            It.IsAny<Expression<Func<AiUsageEntity, object>>>(),
            It.IsAny<Expression<Func<AiUsageEntity, bool>>>(),
            It.IsAny<IDbTransaction>()), Times.Never);
    }

    [TestMethod]
    public async Task CompleteAsync_SucceededClearsErrorType()
    {
        var repository = new Mock<IAiUsageRepository>();
        AiUsageEntity? update = null;
        repository
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiUsageEntity>(),
                It.IsAny<Expression<Func<AiUsageEntity, object>>>(),
                It.IsAny<Expression<Func<AiUsageEntity, bool>>>(),
                null))
            .Callback<AiUsageEntity, Expression<Func<AiUsageEntity, object>>?, Expression<Func<AiUsageEntity, bool>>?, IDbTransaction?>(
                (entity, _, _, _) => update = entity)
            .ReturnsAsync(1);

        await new AiUsageRecorder(repository.Object)
            .CompleteAsync(101, AiUsageStatus.Succeeded, 1, 2, 3, "ai_provider_error");

        Assert.IsNull(update!.ErrorType);
    }

    [TestMethod]
    public async Task CompleteAsync_RetainsBusinessErrorAuditCode()
    {
        var repository = new Mock<IAiUsageRepository>();
        AiUsageEntity? update = null;
        repository
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiUsageEntity>(),
                It.IsAny<Expression<Func<AiUsageEntity, object>>>(),
                It.IsAny<Expression<Func<AiUsageEntity, bool>>>(),
                null))
            .Callback<AiUsageEntity, Expression<Func<AiUsageEntity, object>>?, Expression<Func<AiUsageEntity, bool>>?, IDbTransaction?>(
                (entity, _, _, _) => update = entity)
            .ReturnsAsync(1);

        await new AiUsageRecorder(repository.Object)
            .CompleteAsync(101, AiUsageStatus.Failed, 0, 0, 10, "ai_business_error");

        Assert.AreEqual("ai_business_error", update!.ErrorType);
    }

    [TestMethod]
    public async Task CompleteAsync_MissingOwnedRowThrowsExplicitException()
    {
        var repository = new Mock<IAiUsageRepository>();
        repository
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiUsageEntity>(),
                It.IsAny<Expression<Func<AiUsageEntity, object>>>(),
                It.IsAny<Expression<Func<AiUsageEntity, bool>>>(),
                null))
            .ReturnsAsync(0);

        await Assert.ThrowsAsync<ExplicitException>(() =>
            new AiUsageRecorder(repository.Object)
                .CompleteAsync(101, AiUsageStatus.Failed, 0, 0, 10, "ai_timeout"));
    }

    [TestMethod]
    public async Task CompleteAsync_UnknownErrorTypeUsesProviderErrorAuditCode()
    {
        var repository = new Mock<IAiUsageRepository>();
        AiUsageEntity? update = null;
        repository
            .Setup(item => item.UpdateAsync(
                It.IsAny<AiUsageEntity>(),
                It.IsAny<Expression<Func<AiUsageEntity, object>>>(),
                It.IsAny<Expression<Func<AiUsageEntity, bool>>>(),
                null))
            .Callback<AiUsageEntity, Expression<Func<AiUsageEntity, object>>?, Expression<Func<AiUsageEntity, bool>>?, IDbTransaction?>(
                (entity, _, _, _) => update = entity)
            .ReturnsAsync(1);

        await new AiUsageRecorder(repository.Object)
            .CompleteAsync(101, AiUsageStatus.Failed, 0, 0, 10, "database password leaked");

        Assert.AreEqual("ai_provider_error", update!.ErrorType);
    }
}
