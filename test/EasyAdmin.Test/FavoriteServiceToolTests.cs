using System.Data;
using System.Linq.Expressions;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using Moq;

namespace EasyAdmin.Test;

[TestClass]
public class FavoriteServiceToolTests
{
    private readonly Mock<IFavoriteRepository> _favoriteRepository = new();
    private readonly Mock<IMenuService> _menuService = new();

    [TestInitialize]
    public void Initialize()
    {
        TenantContextHolder.UserInfo = new JwtUserModel { TenantId = 2, UserId = 10 };
        _menuService
            .Setup(service => service.GetMenuTreeAsync(10, It.IsAny<MenuListReqDto>()))
            .ReturnsAsync(new List<MenuEntity>
            {
                new()
                {
                    Id = 11000003,
                    Type = MenuType.Internal,
                    State = CommonState.Enable,
                    Path = ToolboxToolCatalog.ToolboxPath
                }
            });
        _favoriteRepository
            .Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<FavoriteEntity, bool>>>(),
                It.IsAny<Expression<Func<FavoriteEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new FavoriteEntity());
        _favoriteRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<FavoriteEntity>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<FavoriteEntity, object>>>(),
                It.IsAny<IDbTransaction>()))
            .Callback<FavoriteEntity, bool, Expression<Func<FavoriteEntity, object>>, IDbTransaction>(
                (entity, _, _, _) => entity.Id = 99)
            .ReturnsAsync(true);
    }

    [TestCleanup]
    public void Cleanup()
    {
        TenantContextHolder.Clear();
    }

    [TestMethod]
    public async Task AddAsync_AllowsKnownToolWhenToolboxMenuIsAccessible()
    {
        var result = await CreateService().AddAsync(new FavoriteTargetReqDto
        {
            TargetType = FavoriteTargetType.Tool,
            TargetId = 3
        });

        Assert.IsTrue(result.IsFavorite);
        Assert.AreEqual(99, result.FavoriteId);
    }

    private FavoriteService CreateService()
    {
        return new FavoriteService(
            _favoriteRepository.Object,
            _menuService.Object,
            Mock.Of<IFileRepository>(),
            Mock.Of<INoteRepository>(),
            Mock.Of<IShareRepository>(),
            Mock.Of<IUserRepository>(),
            Mock.Of<IShareService>());
    }
}
