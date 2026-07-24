using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Wrapper;
using Moq;
using Sean.Core.Redis;

namespace EasyAdmin.Test;

[TestClass]
public class AuthPasswordVerifierTests
{
    [TestInitialize]
    public void Initialize() => RedisTestSetup.EnsureInitialized();

    [TestCleanup]
    public async Task Cleanup() => await RedisHelper.KeyDeleteAsync("EasyAdmin:LockPasswordFailed:7:");

    [TestMethod]
    public async Task VerifyAsync_WithCorrectPassword_ReturnsTrue()
    {
        var users = new Mock<IUserService>();
        users.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(new UserEntity { Id = 7, Password = "md5-password" });
        var service = new AuthPasswordVerifier(users.Object, Mock.Of<IParamService>());

        Assert.IsTrue(await service.VerifyAsync(7, "md5-password"));
    }

    [TestMethod]
    public async Task VerifyAsync_WithUser_ComparesStoredPasswordWithoutReloadingUser()
    {
        var users = new Mock<IUserService>();
        var service = new AuthPasswordVerifier(
            users.Object,
            Mock.Of<IParamService>());

        Assert.IsTrue(await service.VerifyAsync(new UserEntity { Id = 7, Password = "md5-password" }, "md5-password"));

        users.Verify(x => x.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task VerifyAsync_WithMissingPassword_ThrowsExplicitException(string? password)
    {
        var service = new AuthPasswordVerifier(
            Mock.Of<IUserService>(),
            Mock.Of<IParamService>());

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.VerifyAsync(7, password));
    }

    [TestMethod]
    public async Task VerifyAsync_WithWrongPassword_ThrowsExplicitException()
    {
        var users = new Mock<IUserService>();
        users.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(new UserEntity { Id = 7, Password = "correct" });
        var service = new AuthPasswordVerifier(users.Object, Mock.Of<IParamService>());

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.VerifyAsync(7, "wrong"));
    }

    [TestMethod]
    public async Task VerifyAsync_UsesConfiguredFailureCount()
    {
        var users = new Mock<IUserService>();
        users.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(new UserEntity { Id = 7, Password = "correct" });
        var parameters = new Mock<IParamService>();
        parameters.Setup(x => x.GetValueAsync(ConfigConst.UserPasswordMismatchLockCount)).ReturnsAsync("2");
        parameters.Setup(x => x.GetValueAsync(ConfigConst.UserPasswordMismatchLockMinutes)).ReturnsAsync("5");
        var service = new AuthPasswordVerifier(users.Object, parameters.Object);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.VerifyAsync(7, "wrong"));
        var exception = await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.VerifyAsync(7, "wrong"));

        StringAssert.Contains(exception.Message, "密码错误次数过多");
    }

    [TestMethod]
    public async Task VerifyAsync_WithNonPositiveFailureCount_DisablesLocking()
    {
        var users = new Mock<IUserService>();
        users.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(new UserEntity { Id = 7, Password = "correct" });
        var parameters = new Mock<IParamService>();
        parameters.Setup(x => x.GetValueAsync(ConfigConst.UserPasswordMismatchLockCount)).ReturnsAsync("0");
        parameters.Setup(x => x.GetValueAsync(ConfigConst.UserPasswordMismatchLockMinutes)).ReturnsAsync("1");
        var service = new AuthPasswordVerifier(users.Object, parameters.Object);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.VerifyAsync(7, "wrong"));
        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.VerifyAsync(7, "wrong"));

        users.Verify(x => x.GetByIdAsync(7), Times.Exactly(2));
    }

}
