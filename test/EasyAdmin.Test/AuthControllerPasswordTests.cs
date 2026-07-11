using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Controllers;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasyAdmin.Test;

[TestClass]
public class AuthControllerPasswordTests
{
    [TestCleanup]
    public void Cleanup() => TenantContextHolder.Clear();

    [TestMethod]
    public async Task VerifyPassword_UsesCurrentUserAndReturnsVerifierResult()
    {
        TenantContextHolder.UserInfo = new JwtUserModel { UserId = 7 };
        var users = new Mock<IUserService>();
        users.Setup(x => x.CheckPasswordAsync(7, "md5-password")).ReturnsAsync(true);
        var verifier = new AuthPasswordVerifier(Mock.Of<ILogger<AuthPasswordVerifier>>(), users.Object);
        var controller = CreateController(verifier);

        var result = await controller.VerifyPassword(new VerifyPasswordRequest { Password = "md5-password" });

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Data);
        users.Verify(x => x.CheckPasswordAsync(7, "md5-password"), Times.Once);
    }

    [TestMethod]
    public void VerifyPassword_UsesLockPasswordVerifyRateLimitPolicy()
    {
        var action = typeof(AuthController).GetMethod(nameof(AuthController.VerifyPassword));

        var attribute = action?.GetCustomAttributes(typeof(EnableRateLimitingAttribute), false)
            .Cast<EnableRateLimitingAttribute>()
            .SingleOrDefault();

        Assert.IsNotNull(attribute);
        Assert.AreEqual("LockPasswordVerify", attribute.PolicyName);
    }

    private static AuthController CreateController(AuthPasswordVerifier verifier) => new(
        Mock.Of<ILogger<AuthController>>(),
        Mock.Of<IConfiguration>(),
        Mock.Of<IUserService>(),
        Mock.Of<ILoginLogService>(),
        Mock.Of<ITokenService>(),
        Mock.Of<ICaptchaService>(),
        Mock.Of<IParamService>(),
        Mock.Of<ITenantService>(),
        Mock.Of<IAccountAccessService>(),
        verifier);
}
