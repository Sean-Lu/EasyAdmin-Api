using System.Linq.Expressions;
using System.Data;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Sean.Utility.Security.Provider;

namespace EasyAdmin.Test;

[TestClass]
public class UserRegistrationServiceTests
{
    [TestMethod]
    public async Task RegisterAsync_HashesPasswordOnce()
    {
        var users = new Mock<IUserRepository>();
        var roles = new Mock<IRoleRepository>();
        var userRoles = new Mock<IUserRoleRepository>();
        UserEntity? createdUser = null;

        users.Setup(repository => repository.ExistsAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), false))
            .ReturnsAsync(false);
        roles.Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<RoleEntity, bool>>>(),
                It.IsAny<Expression<Func<RoleEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new RoleEntity { Id = 1000006, TenantId = 1, Code = SysConst.NormalUserRoleCode, State = CommonState.Enable });
        users.Setup(repository => repository.ExecuteAutoTransactionAsync(It.IsAny<Func<IDbTransaction, Task<bool>>>(), null, null))
            .Returns((Func<IDbTransaction, Task<bool>> callback, IDbTransaction? _, IDbConnection? __) => callback(Mock.Of<IDbTransaction>()));
        users.Setup(repository => repository.AddAsync(It.IsAny<UserEntity>(), false, null, It.IsAny<IDbTransaction>()))
            .Callback<UserEntity, bool, Expression<Func<UserEntity, object>>?, IDbTransaction>((entity, _, _, __) => createdUser = entity)
            .ReturnsAsync(true);
        userRoles.Setup(repository => repository.AddAsync(It.IsAny<UserRoleEntity>(), false, null, It.IsAny<IDbTransaction>()))
            .ReturnsAsync(true);

        var service = new UserService(
            Mock.Of<ILogger<UserService>>(),
            Mock.Of<IMapper>(),
            users.Object,
            Mock.Of<IParamRepository>(),
            roles.Object,
            userRoles.Object);

        var password = new HashCryptoProvider().MD5("123456")?.ToLower();
        await service.RegisterAsync(new RegisterUserDto { UserName = "new-user", Password = password! }, tenantId: 1);

        Assert.IsNotNull(createdUser);
        Assert.AreEqual(password, createdUser.Password);
    }

    [TestMethod]
    public async Task RegisterAsync_RejectsDuplicateUsernameWithinTenant()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.ExistsAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), false))
            .ReturnsAsync(true);

        var service = new UserService(
            Mock.Of<ILogger<UserService>>(),
            Mock.Of<IMapper>(),
            users.Object,
            Mock.Of<IParamRepository>(),
            Mock.Of<IRoleRepository>(),
            Mock.Of<IUserRoleRepository>());

        await Assert.ThrowsAsync<ExplicitException>(() => service.RegisterAsync(
            new RegisterUserDto { UserName = "duplicate", Password = "password" },
            tenantId: 1));
    }
}
