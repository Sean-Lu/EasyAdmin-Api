using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Wrapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EasyAdmin.Test;

[TestClass]
public class AuthPasswordVerifierTests
{
    [TestMethod]
    public async Task VerifyAsync_WithCorrectPassword_ReturnsTrue()
    {
        var users = new Mock<IUserService>();
        users.Setup(x => x.CheckPasswordAsync(7, "md5-password")).ReturnsAsync(true);
        var service = new AuthPasswordVerifier(NullLogger<AuthPasswordVerifier>.Instance, users.Object);

        Assert.IsTrue(await service.VerifyAsync(7, "md5-password"));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task VerifyAsync_WithMissingPassword_ThrowsExplicitException(string? password)
    {
        var service = new AuthPasswordVerifier(
            NullLogger<AuthPasswordVerifier>.Instance,
            Mock.Of<IUserService>());

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.VerifyAsync(7, password));
    }

    [TestMethod]
    public async Task VerifyAsync_WithWrongPassword_ThrowsExplicitException()
    {
        var users = new Mock<IUserService>();
        users.Setup(x => x.CheckPasswordAsync(7, "wrong")).ReturnsAsync(false);
        var logger = new CapturingLogger<AuthPasswordVerifier>();
        var service = new AuthPasswordVerifier(logger, users.Object);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.VerifyAsync(7, "wrong"));

        Assert.IsTrue(logger.Messages.Count > 0);
        Assert.IsFalse(logger.Messages.Any(message => message.Contains("wrong", StringComparison.Ordinal)));
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
