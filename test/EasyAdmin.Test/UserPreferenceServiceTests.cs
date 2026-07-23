using System.Data;
using System.Linq.Expressions;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Wrapper;
using Moq;

namespace EasyAdmin.Test;

[TestClass]
public class UserPreferenceServiceTests
{
    [TestMethod]
    public async Task GetToolboxToolOrderAsync_NormalizesStoredOrder()
    {
        var repository = new Mock<IUserPreferenceRepository>();
        repository.Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<UserPreferenceEntity, bool>>>(),
                It.IsAny<Expression<Func<UserPreferenceEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new UserPreferenceEntity { Id = 1, PreferenceValue = "[3,999,3,1]" });

        var result = await new UserPreferenceService(repository.Object).GetToolboxToolOrderAsync();

        CollectionAssert.AreEqual(
            new long[] { 3, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
            result.ToolIds);
    }

    [TestMethod]
    public async Task UpdateToolboxToolOrderAsync_RejectsDuplicateOrUnknownTools()
    {
        var service = new UserPreferenceService(Mock.Of<IUserPreferenceRepository>());

        await Assert.ThrowsAsync<ExplicitException>(() =>
            service.UpdateToolboxToolOrderAsync(new ToolboxToolOrderDto { ToolIds = new long[] { 3, 3 }.ToList() }));
        await Assert.ThrowsAsync<ExplicitException>(() =>
            service.UpdateToolboxToolOrderAsync(new ToolboxToolOrderDto { ToolIds = new long[] { 3, 999 }.ToList() }));
    }

    [TestMethod]
    public async Task UpdateToolboxToolOrderAsync_AppendsToolsMissingFromOlderClients()
    {
        var repository = new Mock<IUserPreferenceRepository>();
        repository.Setup(item => item.GetAsync(
                It.IsAny<Expression<Func<UserPreferenceEntity, bool>>>(),
                It.IsAny<Expression<Func<UserPreferenceEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new UserPreferenceEntity());
        repository.Setup(item => item.AddAsync(
                It.IsAny<UserPreferenceEntity>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<UserPreferenceEntity, object>>>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(true);

        var result = await new UserPreferenceService(repository.Object).UpdateToolboxToolOrderAsync(
            new ToolboxToolOrderDto { ToolIds = new long[] { 3, 1 }.ToList() });

        CollectionAssert.AreEqual(
            new long[] { 3, 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
            result.ToolIds);
    }
}
