using System.Security.Claims;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Middleware;
using Microsoft.AspNetCore.Http;

namespace EasyAdmin.Test;

[TestClass]
public class AccountAccessMiddlewareTests
{
    [TestMethod]
    public async Task InvokeAsync_WhenAccountDisabled_ReturnsUnauthorized()
    {
        var nextCalled = false;
        var middleware = new AccountAccessMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateAuthenticatedContext(2, 10);

        await middleware.InvokeAsync(context, new FakeAccountAccessService(false));

        Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.IsFalse(nextCalled);
    }

    [TestMethod]
    public async Task InvokeAsync_WhenAccountEnabled_ContinuesPipeline()
    {
        var nextCalled = false;
        var middleware = new AccountAccessMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = CreateAuthenticatedContext(2, 10);

        await middleware.InvokeAsync(context, new FakeAccountAccessService(true));

        Assert.IsTrue(nextCalled);
    }

    private static DefaultHttpContext CreateAuthenticatedContext(long tenantId, long userId)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(nameof(JwtUserModel.TenantId), tenantId.ToString()),
            new Claim(nameof(JwtUserModel.UserId), userId.ToString())
        }, "test");
        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    private sealed class FakeAccountAccessService(bool allowed) : IAccountAccessService
    {
        public Task<bool> IsAllowedAsync(long tenantId, long userId) => Task.FromResult(allowed);
        public Task InvalidateTenantAsync(long tenantId) => Task.CompletedTask;
        public Task InvalidateUserAsync(long tenantId, long userId) => Task.CompletedTask;
    }
}
