using System.Security.Claims;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Controllers;
using EasyAdmin.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sean.Core.Redis;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Web.Helper;

namespace EasyAdmin.Test;

[TestClass]
public class AuthControllerPasswordTests
{
    [TestInitialize]
    public void Initialize() => RedisTestSetup.EnsureInitialized();

    [TestCleanup]
    public async Task Cleanup()
    {
        TenantContextHolder.Clear();
        await RedisHelper.KeyDeleteAsync("EasyAdmin:LockPasswordFailed:7:");
    }

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
    public async Task Logout_RevokesTheSingleTokenSession()
    {
        var previousConfig = JwtHelper.JwtConfig;
        try
        {
            JwtHelper.JwtConfig = new JwtConfig
            {
                SecretKey = "test-secret-key-which-is-long-enough",
                Issuer = "test-issuer",
                Audience = "test-audience",
                TokenMode = TokenMode.Single,
                Expired = 30
            };
            var tokenService = new Mock<ITokenService>();
            var verifier = new AuthPasswordVerifier(Mock.Of<ILogger<AuthPasswordVerifier>>(), Mock.Of<IUserService>());
            var controller = CreateController(verifier, tokenService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(nameof(JwtUserModel.UserId), "7"),
                        new Claim(nameof(JwtUserModel.TenantId), "1")
                    ], "test"))
                }
            };
            controller.Request.Headers["Authorization"] = "Bearer single-token";

            await controller.Logout();

            tokenService.Verify(x => x.RevokeUserSessionsAsync(7, "用户主动登出"), Times.Once);
            tokenService.Verify(x => x.AddTokenToBlacklistAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        finally
        {
            JwtHelper.JwtConfig = previousConfig;
        }
    }

    private static AuthController CreateController(AuthPasswordVerifier verifier, ITokenService? tokenService = null) => new(
        Mock.Of<ILogger<AuthController>>(),
        Mock.Of<IConfiguration>(),
        Mock.Of<IUserService>(),
        Mock.Of<ILoginLogService>(),
        tokenService ?? Mock.Of<ITokenService>(),
        Mock.Of<ICaptchaService>(),
        Mock.Of<IParamService>(),
        Mock.Of<ITenantService>(),
        Mock.Of<IAccountAccessService>(),
        verifier);
}
