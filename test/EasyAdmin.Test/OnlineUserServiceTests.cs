using EasyAdmin.Application.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Models;
using EasyAdmin.Web.Services;
using Moq;

namespace EasyAdmin.Test;

[TestClass]
public class OnlineUserServiceTests
{
    [TestMethod]
    public async Task PageAsync_FiltersByTenantAndUserNameBeforePaging()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService
            .Setup(service => service.GetOnlineSessionRecordsAsync(11))
            .ReturnsAsync(new[]
            {
                new OnlineUserSessionRecord(7, 11, "10.0.0.1", DateTime.UtcNow, "Chrome", DateTime.UtcNow.AddMinutes(10)),
                new OnlineUserSessionRecord(8, 22, "10.0.0.2", DateTime.UtcNow, "Edge", DateTime.UtcNow.AddMinutes(10))
            });
        var userService = new Mock<IUserService>();
        userService.Setup(service => service.GetByIdAsync(7)).ReturnsAsync(new UserEntity
        {
            Id = 7,
            TenantId = 11,
            UserName = "alice",
            NickName = "Alice"
        });

        var service = new OnlineUserService(userService.Object, tokenService.Object);

        var result = await service.PageAsync(new OnlineUserPageRequest { UserName = "ali", PageNumber = 1, PageSize = 10 }, 11);

        Assert.AreEqual(1, result.Total);
        Assert.AreEqual(7, result.List[0].UserId);
    }

    [TestMethod]
    public async Task KickAsync_RejectsUserFromAnotherTenant()
    {
        var userService = new Mock<IUserService>();
        userService.Setup(service => service.GetByIdAsync(7)).ReturnsAsync(new UserEntity { Id = 7, TenantId = 22 });
        var service = new OnlineUserService(userService.Object, Mock.Of<ITokenService>());

        try
        {
            await service.KickAsync(7, 11);
            Assert.Fail("Expected ExplicitException");
        }
        catch (ExplicitException)
        {
        }
    }
}
