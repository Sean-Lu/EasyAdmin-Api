using System.Data;
using System.Linq.Expressions;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Moq;

namespace EasyAdmin.Test;

[TestClass]
public class TodoItemServiceTests
{
    [TestInitialize]
    public void Initialize()
    {
        TenantContextHolder.UserInfo = new JwtUserModel { TenantId = 7, UserId = 11 };
    }

    [TestCleanup]
    public void Cleanup()
    {
        TenantContextHolder.Clear();
    }

    [TestMethod]
    public async Task AddAsync_RejectsCategoryOutsideCurrentOwner()
    {
        var mapper = new Mock<IMapper>();
        mapper
            .Setup(item => item.Map<TodoItemEntity>(It.IsAny<object>()))
            .Returns(new TodoItemEntity());
        var items = new Mock<ITodoItemRepository>();
        items
            .Setup(item => item.AddAsync(
                It.IsAny<TodoItemEntity>(),
                false,
                null,
                null))
            .ReturnsAsync(true);
        var categories = new Mock<ITodoCategoryRepository>();
        categories
            .Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<TodoCategoryEntity, bool>>>(),
                It.IsAny<Expression<Func<TodoCategoryEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((TodoCategoryEntity)null!);
        var service = new TodoItemService(mapper.Object, items.Object, categories.Object);

        await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.AddAsync(new TodoItemDto
        {
            CategoryId = 99,
            Name = "待办",
            Priority = 1
        }));

        items.Verify(item => item.AddAsync(
            It.IsAny<TodoItemEntity>(),
            It.IsAny<bool>(),
            It.IsAny<Expression<Func<TodoItemEntity, object>>>(),
            It.IsAny<IDbTransaction>()), Times.Never);
    }
}
