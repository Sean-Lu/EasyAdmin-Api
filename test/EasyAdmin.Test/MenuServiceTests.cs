using System.Data;
using System.Linq.Expressions;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasyAdmin.Test;

[TestClass]
public class MenuServiceTests
{
    private readonly Mock<IMenuRepository> _menuRepository = new();
    private readonly Mock<IMapper> _mapper = new();

    [TestInitialize]
    public void Initialize()
    {
        TenantContextHolder.UserInfo = new JwtUserModel { TenantId = 2, UserId = 10 };
        _menuRepository
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Expression<Func<MenuEntity, bool>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(false);
        _menuRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<MenuEntity>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<MenuEntity, object>>>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(true);
        _mapper
            .Setup(mapper => mapper.Map<MenuEntity>(It.IsAny<MenuDto>()))
            .Returns((MenuDto dto) => new MenuEntity
            {
                PId = dto.PId,
                Title = dto.Title,
                Type = dto.Type,
                Path = dto.Path,
                OutLink = dto.OutLink,
                OutLinkOpenType = dto.OutLinkOpenType,
                State = dto.State
            });
    }

    [TestCleanup]
    public void Cleanup()
    {
        TenantContextHolder.Clear();
    }

    [TestMethod]
    public async Task AddAsync_AssignsCurrentTenant()
    {
        MenuEntity? added = null;
        _menuRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<MenuEntity>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<MenuEntity, object>>>(),
                It.IsAny<IDbTransaction>()))
            .Callback<MenuEntity, bool, Expression<Func<MenuEntity, object>>, IDbTransaction>(
                (entity, _, _, _) => added = entity)
            .ReturnsAsync(true);

        await CreateService().AddAsync(new MenuDto
        {
            PId = 0,
            Title = "租户目录",
            Type = MenuType.Directory,
            State = CommonState.Enable
        });

        Assert.IsNotNull(added);
        Assert.AreEqual(2, added.TenantId);
    }

    [TestMethod]
    public async Task AddAsync_RejectsLeafParent()
    {
        _menuRepository
            .Setup(repository => repository.GetByIdAsync(5))
            .ReturnsAsync(new MenuEntity { Id = 5, TenantId = 2, Type = MenuType.Internal, Path = "/parent" });

        await Assert.ThrowsAsync<ExplicitException>(() => CreateService().AddAsync(new MenuDto
        {
            PId = 5,
            Title = "子菜单",
            Type = MenuType.Internal,
            Path = "/child",
            State = CommonState.Enable
        }));
    }

    [TestMethod]
    public async Task AddAsync_AllowsSameRouteInDifferentTenant()
    {
        ConfigureExistingMenus(new MenuEntity
        {
            Id = 8,
            TenantId = 3,
            Type = MenuType.External,
            Path = "/link/GitHub"
        });

        var result = await CreateService().AddAsync(new MenuDto
        {
            PId = 0,
            Title = "GitHub",
            Type = MenuType.External,
            Path = "/link/github",
            OutLink = "https://github.com",
            OutLinkOpenType = OutLinkOpenType.Blank,
            State = CommonState.Enable
        });

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AddAsync_AllowsGlobalRouteWithDifferentCase()
    {
        ConfigureExistingMenus(new MenuEntity
        {
            Id = 8,
            TenantId = null,
            Type = MenuType.External,
            Path = "/link/GitHub"
        });

        var result = await CreateService().AddAsync(new MenuDto
        {
            PId = 0,
            Title = "GitHub",
            Type = MenuType.External,
            Path = "/link/github",
            OutLink = "https://github.com",
            OutLinkOpenType = OutLinkOpenType.Blank,
            State = CommonState.Enable
        });

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteByIdAsync_TenantUserCannotDeleteGlobalMenu()
    {
        _menuRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<long>>()))
            .ReturnsAsync(new List<MenuEntity> { new() { Id = 5, TenantId = null, Type = MenuType.Directory } });
        _menuRepository.Setup(repository => repository.DeleteByIdAsync(5, null)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ExplicitException>(() => CreateService().DeleteByIdAsync(5));

        StringAssert.Contains(exception.Message, "无权修改");
    }

    [TestMethod]
    public async Task UpdateAsync_SuperAdminCanChangeGlobalMenuNavigation()
    {
        TenantContextHolder.UserInfo = new JwtUserModel
        {
            TenantId = SysConst.DefaultTenantId,
            UserId = SysConst.SuperAdminUserId
        };
        _menuRepository
            .Setup(repository => repository.GetByIdAsync(5))
            .ReturnsAsync(new MenuEntity
            {
                Id = 5,
                TenantId = null,
                PId = 0,
                Type = MenuType.Internal,
                Title = "旧菜单",
                Path = "/system/old"
            });
        _mapper
            .Setup(mapper => mapper.Map<MenuEntity>(It.IsAny<MenuUpdateDto>()))
            .Returns((MenuUpdateDto dto) => new MenuEntity
            {
                Id = dto.Id,
                PId = dto.PId,
                Type = dto.Type,
                Title = dto.Title,
                Path = dto.Path,
                OutLink = dto.OutLink,
                OutLinkOpenType = dto.OutLinkOpenType,
                State = dto.State
            });

        await CreateService().UpdateAsync(new MenuUpdateDto
        {
            Id = 5,
            PId = 0,
            Type = MenuType.External,
            Title = "新菜单",
            Path = "/link/GitHub",
            OutLink = "https://github.com",
            OutLinkOpenType = OutLinkOpenType.Blank,
            State = CommonState.Enable
        });
    }

    private MenuService CreateService()
    {
        return new MenuService(
            Mock.Of<ILogger<MenuService>>(),
            _mapper.Object,
            _menuRepository.Object,
            Mock.Of<IUserRoleRepository>(),
            Mock.Of<IRoleRepository>(),
            Mock.Of<IRoleMenuRepository>());
    }

    private void ConfigureExistingMenus(params MenuEntity[] menus)
    {
        _menuRepository
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Expression<Func<MenuEntity, bool>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Expression<Func<MenuEntity, bool>> expression, bool _) =>
                menus.Any(expression.Compile()));
    }
}