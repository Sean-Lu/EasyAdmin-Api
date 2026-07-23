using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Controllers;
using EasyAdmin.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sean.Core.Redis;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Test;

[TestClass]
public class AuthRegistrationTests
{
    [TestInitialize]
    public void Initialize() => RedisTestSetup.EnsureInitialized();

    [TestMethod]
    public async Task LoginConfig_ReturnsRegistrationFlag()
    {
        var parameters = new Mock<IParamService>();
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.TenantEnable, false)).ReturnsAsync(false);
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.UserEnableRegister, false)).ReturnsAsync(true);
        var controller = CreateController(parameters.Object);

        var result = await controller.LoginConfig();

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Data.RegisterEnabled);
    }

    [TestMethod]
    public async Task Register_WhenDisabledRejectsRequest()
    {
        var parameters = new Mock<IParamService>();
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.UserEnableRegister, false)).ReturnsAsync(false);
        var captcha = new Mock<ICaptchaService>();
        var controller = CreateController(parameters.Object, captcha.Object);

        var result = await controller.Register(new RegisterRequest
        {
            UserName = "new-user",
            Password = "password"
        });

        Assert.IsFalse(result.Success);
        captcha.Verify(service => service.ValidateAsync(It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
    }

    [TestMethod]
    public async Task Register_WhenEnabledValidatesCaptchaAndCreatesUserWithoutLoggingIn()
    {
        var parameters = new Mock<IParamService>();
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.UserEnableRegister, false)).ReturnsAsync(true);
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.TenantEnable, false)).ReturnsAsync(false);
        var captcha = new Mock<ICaptchaService>();
        captcha.Setup(service => service.ValidateAsync("captcha-key", "1234")).ReturnsAsync(true);
        var users = new Mock<IUserService>();
        users.Setup(service => service.RegisterAsync(It.IsAny<RegisterUserDto>(), 1)).ReturnsAsync(new EasyAdmin.Domain.Entities.UserEntity
        {
            Id = 3,
            UserName = "new-user",
            PhoneNumber = "13800138000",
            Email = "new-user@example.com",
            ApprovalState = UserApprovalState.Pending
        });
        var logs = new Mock<ILoginLogService>();
        var tokens = new Mock<ITokenService>();
        var tenants = new Mock<ITenantService>();
        tenants.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(new EasyAdmin.Domain.Entities.TenantEntity
        {
            Id = 1,
            Code = "default",
            State = EasyAdmin.Infrastructure.Enums.CommonState.Enable
        });
        var controller = CreateController(parameters.Object, captcha.Object, users.Object, logs.Object, tokens.Object, tenants.Object);

        var result = await controller.Register(new RegisterRequest
        {
            UserName = "new-user",
            Password = "password",
            PhoneNumber = "13800138000",
            Email = "new-user@example.com",
            CaptchaKey = "captcha-key",
            CaptchaCode = "1234"
        });

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Data.RequiresApproval);
        Assert.AreEqual("new-user", result.Data.UserName);
        Assert.AreEqual("13800138000", result.Data.PhoneNumber);
        Assert.AreEqual("new-user@example.com", result.Data.Email);
        Assert.AreEqual("default", result.Data.TenantCode);
        users.Verify(service => service.RegisterAsync(
            It.Is<RegisterUserDto>(dto => dto.UserName == "new-user" && dto.PhoneNumber == "13800138000" && dto.Email == "new-user@example.com"), 1), Times.Once);
        logs.Verify(service => service.AddAsync(It.IsAny<LoginLogDto>()), Times.Never);
        tokens.VerifyNoOtherCalls();
    }

    private static AuthController CreateController(
        IParamService parameters,
        ICaptchaService? captcha = null,
        IUserService? users = null,
        ILoginLogService? logs = null,
        ITokenService? tokens = null,
        ITenantService? tenants = null) => new(
        Mock.Of<ILogger<AuthController>>(),
        Mock.Of<IConfiguration>(),
        users ?? Mock.Of<IUserService>(),
        logs ?? Mock.Of<ILoginLogService>(),
        tokens ?? Mock.Of<ITokenService>(),
        captcha ?? Mock.Of<ICaptchaService>(),
        parameters,
        tenants ?? Mock.Of<ITenantService>(),
        Mock.Of<IAccountAccessService>(),
        new AuthPasswordVerifier(Mock.Of<ILogger<AuthPasswordVerifier>>(), Mock.Of<IUserService>()));
}
