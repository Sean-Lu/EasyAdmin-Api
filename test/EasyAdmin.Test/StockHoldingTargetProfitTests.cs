using System.Data;
using System.Linq.Expressions;
using EasyAdmin.Application.Contracts;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Contracts;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Infrastructure.Tenant;
using EasyAdmin.Infrastructure.Wrapper;
using MapsterMapper;
using Moq;
using Sean.Core.DbRepository;

namespace EasyAdmin.Test;

[TestClass]
public class StockHoldingTargetProfitTests
{
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IStockHoldingRepository> _holdingRepository = new();
    private readonly Mock<IStockAccountService> _accountService = new();

    [TestInitialize]
    public void Initialize()
    {
        TenantContextHolder.UserInfo = new JwtUserModel { TenantId = 7, UserId = 11 };
        _accountService.Setup(service => service.ExistsForCurrentUserAsync(20)).ReturnsAsync(true);
    }

    [TestCleanup]
    public void Cleanup()
    {
        TenantContextHolder.Clear();
    }

    [TestMethod]
    public async Task ListAsync_CalculatesTargetPriceAndKeepsUnsetTargetEmpty()
    {
        _holdingRepository
            .Setup(repository => repository.QueryAsync(
                It.IsAny<Expression<Func<StockHoldingEntity, bool>>>(),
                It.IsAny<OrderByCondition>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Expression<Func<StockHoldingEntity, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new List<StockHoldingEntity>
            {
                new()
                {
                    Id = 1,
                    AccountId = 20,
                    UserId = 11,
                    TenantId = 7,
                    Name = "目标持仓",
                    Code = "000001",
                    CostPrice = 10m,
                    Quantity = 3000m,
                    CurrentPrice = 12m,
                    TargetProfitAmount = 10000m,
                    IsEnabled = true
                },
                new()
                {
                    Id = 2,
                    AccountId = 20,
                    UserId = 11,
                    TenantId = 7,
                    Name = "普通持仓",
                    Code = "000002",
                    CostPrice = 8m,
                    Quantity = 100m,
                    CurrentPrice = 9m,
                    IsEnabled = true
                }
            });

        var result = await CreateService().ListAsync(20, null);

        Assert.AreEqual(10000m, result.List[0].TargetProfitAmount);
        Assert.AreEqual(13.334m, result.List[0].TargetPrice);
        Assert.IsNull(result.List[1].TargetProfitAmount);
        Assert.IsNull(result.List[1].TargetPrice);
    }

    [TestMethod]
    public async Task AddAsync_PersistsTargetProfitAmount()
    {
        StockHoldingEntity? added = null;
        _mapper
            .Setup(mapper => mapper.Map<StockHoldingEntity>(It.IsAny<StockHoldingDto>()))
            .Returns(new StockHoldingEntity());
        _holdingRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<StockHoldingEntity>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<StockHoldingEntity, object>>>(),
                It.IsAny<IDbTransaction>()))
            .Callback<StockHoldingEntity, bool, Expression<Func<StockHoldingEntity, object>>, IDbTransaction>(
                (entity, _, _, _) => added = entity)
            .ReturnsAsync(true);

        var result = await CreateService().AddAsync(ValidHolding(10000m));

        Assert.IsTrue(result);
        Assert.AreEqual(10000m, added?.TargetProfitAmount);
    }

    [TestMethod]
    public async Task UpdateAsync_ClearsTargetProfitAmount()
    {
        StockHoldingEntity? update = null;
        Expression<Func<StockHoldingEntity, object>>? fields = null;
        _holdingRepository
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<StockHoldingEntity>(),
                It.IsAny<Expression<Func<StockHoldingEntity, object>>>(),
                It.IsAny<Expression<Func<StockHoldingEntity, bool>>>(),
                It.IsAny<IDbTransaction>()))
            .Callback<StockHoldingEntity, Expression<Func<StockHoldingEntity, object>>, Expression<Func<StockHoldingEntity, bool>>, IDbTransaction>(
                (entity, selectedFields, _, _) =>
                {
                    update = entity;
                    fields = selectedFields;
                })
            .ReturnsAsync(1);

        var result = await CreateService().UpdateAsync(new StockHoldingUpdateDto
        {
            Id = 1,
            AccountId = 20,
            Name = "目标持仓",
            Code = "000001",
            CostPrice = 10m,
            Quantity = 100m,
            CurrentPrice = 12m,
            TargetProfitAmount = null,
            IsEnabled = true
        });

        Assert.IsTrue(result);
        Assert.IsNull(update?.TargetProfitAmount);
        var selectedFields = fields!.Compile()(new StockHoldingEntity { TargetProfitAmount = 10000m });
        Assert.IsNotNull(selectedFields.GetType().GetProperty(nameof(StockHoldingEntity.TargetProfitAmount)));
    }

    [TestMethod]
    public async Task AddAsync_RejectsNonpositiveTargetProfitAmount()
    {
        foreach (var targetProfitAmount in new[] { 0m, -1m })
        {
            var exception = await Assert.ThrowsExactlyAsync<ExplicitException>(() =>
                CreateService().AddAsync(ValidHolding(targetProfitAmount)));

            StringAssert.Contains(exception.Message, "目标盈利金额必须大于0");
        }
    }

    [TestMethod]
    public async Task AddAsync_RejectsTargetWhenQuantityIsZero()
    {
        var dto = ValidHolding(10000m);
        dto.Quantity = 0;

        var exception = await Assert.ThrowsExactlyAsync<ExplicitException>(() => CreateService().AddAsync(dto));

        StringAssert.Contains(exception.Message, "持仓数量必须大于0");
    }

    private StockHoldingService CreateService()
    {
        return new StockHoldingService(
            _mapper.Object,
            _holdingRepository.Object,
            _accountService.Object,
            Mock.Of<IStockQuoteProvider>());
    }

    private static StockHoldingDto ValidHolding(decimal? targetProfitAmount)
    {
        return new StockHoldingDto
        {
            AccountId = 20,
            Name = "目标持仓",
            Code = "000001",
            CostPrice = 10m,
            Quantity = 100m,
            CurrentPrice = 12m,
            TargetProfitAmount = targetProfitAmount,
            IsEnabled = true
        };
    }
}
