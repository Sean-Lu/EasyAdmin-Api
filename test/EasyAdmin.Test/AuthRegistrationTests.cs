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
using EasyAdmin.Infrastructure.Wrapper;

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

    [TestMethod]
    public async Task Login_PasswordMode_UsesPasswordVerifierAfterAccountLookup()
    {
        var parameters = new Mock<IParamService>();
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.TenantEnable, false)).ReturnsAsync(false);
        parameters.Setup(service => service.GetValueAsync(ConfigConst.UserPasswordMismatchLockCount)).ReturnsAsync("5");
        parameters.Setup(service => service.GetValueAsync(ConfigConst.UserPasswordMismatchLockMinutes)).ReturnsAsync("1");
        var captcha = new Mock<ICaptchaService>();
        captcha.Setup(service => service.ValidateAsync("captcha-key", "1234")).ReturnsAsync(true);
        var users = new Mock<IUserService>();
        users.Setup(service => service.GetByAccountAsync("user", LoginType.Password, 1)).ReturnsAsync(new EasyAdmin.Domain.Entities.UserEntity
        {
            Id = 7,
            TenantId = 1,
            State = CommonState.Enable,
            ApprovalState = UserApprovalState.NotRequired
        });
        var tenants = new Mock<ITenantService>();
        tenants.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(new EasyAdmin.Domain.Entities.TenantEntity
        {
            Id = 1,
            Code = "default",
            State = CommonState.Enable
        });
        var verifier = new AuthPasswordVerifier(users.Object, parameters.Object);
        var controller = CreateController(parameters.Object, captcha.Object, users.Object, tenants: tenants.Object, verifier: verifier);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => controller.Login(new LoginRequest
        {
            Account = "user",
            Password = "wrong",
            CaptchaKey = "captcha-key",
            CaptchaCode = "1234"
        }));

        users.Verify(service => service.GetByAccountAsync("user", LoginType.Password, 1), Times.Once);
        users.Verify(service => service.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [TestMethod]
    public async Task Login_WhenAccountCannotBeFound_ReturnsAccountLookupMessage()
    {
        var parameters = new Mock<IParamService>();
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.TenantEnable, false)).ReturnsAsync(false);
        var captcha = new Mock<ICaptchaService>();
        captcha.Setup(service => service.ValidateAsync("captcha-key", "1234")).ReturnsAsync(true);
        var tenants = new Mock<ITenantService>();
        tenants.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(new EasyAdmin.Domain.Entities.TenantEntity
        {
            Id = 1,
            Code = "default",
            State = CommonState.Enable
        });
        var controller = CreateController(parameters.Object, captcha.Object, tenants: tenants.Object);

        var result = await controller.Login(new LoginRequest
        {
            Account = "missing-user",
            Password = "password",
            CaptchaKey = "captcha-key",
            CaptchaCode = "1234"
        });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("账号不存在", result.Msg);
    }

    [TestMethod]
    public async Task Login_AllowsTenantCodeLongerThanFiftyCharacters()
    {
        var tenantCode = new string('a', 51);
        var parameters = new Mock<IParamService>();
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.TenantEnable, false)).ReturnsAsync(true);
        var captcha = new Mock<ICaptchaService>();
        captcha.Setup(service => service.ValidateAsync("captcha-key", "1234")).ReturnsAsync(true);
        var tenants = new Mock<ITenantService>();
        tenants.Setup(service => service.GetByCodeAsync(tenantCode)).ReturnsAsync(new EasyAdmin.Domain.Entities.TenantEntity
        {
            Id = 2,
            Code = tenantCode,
            State = CommonState.Enable
        });
        var controller = CreateController(parameters.Object, captcha.Object, tenants: tenants.Object);

        var result = await controller.Login(new LoginRequest
        {
            TenantCode = tenantCode,
            Account = "missing-user",
            Password = "password",
            CaptchaKey = "captcha-key",
            CaptchaCode = "1234"
        });

        Assert.IsFalse(result.Success);
        Assert.AreEqual("账号不存在", result.Msg);
        tenants.Verify(service => service.GetByCodeAsync(tenantCode), Times.Once);
    }

    [TestMethod]
    public async Task Login_DoesNotVerifyPasswordBeforeRejectingDisabledUser()
    {
        var parameters = new Mock<IParamService>();
        parameters.Setup(service => service.GetBooleanValueAsync(ConfigConst.TenantEnable, false)).ReturnsAsync(false);
        var captcha = new Mock<ICaptchaService>();
        captcha.Setup(service => service.ValidateAsync("captcha-key", "1234")).ReturnsAsync(true);
        var users = new Mock<IUserService>();
        users.Setup(service => service.GetByAccountAsync("disabled-user", LoginType.Password, 1)).ReturnsAsync(new EasyAdmin.Domain.Entities.UserEntity
        {
            Id = 7,
            TenantId = 1,
            State = CommonState.Disable,
            ApprovalState = UserApprovalState.NotRequired
        });
        var tenants = new Mock<ITenantService>();
        tenants.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(new EasyAdmin.Domain.Entities.TenantEntity
        {
            Id = 1,
            Code = "default",
            State = CommonState.Enable
        });
        var verifier = new AuthPasswordVerifier(users.Object, parameters.Object);
        var controller = CreateController(parameters.Object, captcha.Object, users.Object, tenants: tenants.Object, verifier: verifier);

        var result = await controller.Login(new LoginRequest
        {
            Account = "disabled-user",
            Password = "wrong",
            CaptchaKey = "captcha-key",
            CaptchaCode = "1234"
        });

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Msg, "禁用");
        users.Verify(service => service.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    private static AuthController CreateController(
        IParamService parameters,
        ICaptchaService? captcha = null,
        IUserService? users = null,
        ILoginLogService? logs = null,
        ITokenService? tokens = null,
        ITenantService? tenants = null,
        AuthPasswordVerifier? verifier = null) => new(
        Mock.Of<ILogger<AuthController>>(),
        Mock.Of<IConfiguration>(),
        users ?? Mock.Of<IUserService>(),
        logs ?? Mock.Of<ILoginLogService>(),
        tokens ?? Mock.Of<ITokenService>(),
        captcha ?? Mock.Of<ICaptchaService>(),
        parameters,
        tenants ?? Mock.Of<ITenantService>(),
        Mock.Of<IAccountAccessService>(),
        verifier ?? new AuthPasswordVerifier(Mock.Of<IUserService>(), Mock.Of<IParamService>()));
}
