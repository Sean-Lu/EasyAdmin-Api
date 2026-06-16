using System.Reflection;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace EasyAdmin.Test;

[TestClass]
public class LotteryRepositoryTests
{
    [TestMethod]
    public void Lottery_Repositories_Use_Logical_Delete()
    {
        var configuration = new ConfigurationBuilder().Build();

        AssertUsesLogicalDelete(new LotteryActivityRepository(configuration, NullLogger<LotteryActivityRepository>.Instance));
        AssertUsesLogicalDelete(new LotteryPrizeRepository(configuration, NullLogger<LotteryPrizeRepository>.Instance));
        AssertUsesLogicalDelete(new LotteryParticipantRepository(configuration, NullLogger<LotteryParticipantRepository>.Instance));
        AssertUsesLogicalDelete(new LotteryWinnerRepository(configuration, NullLogger<LotteryWinnerRepository>.Instance));
    }

    private static void AssertUsesLogicalDelete<TEntity>(BaseRepositoryExt<TEntity> repository)
        where TEntity : EntityBase, new()
    {
        var property = typeof(BaseRepositoryExt<TEntity>).GetProperty("IsLogicallyDelete", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(property);
        Assert.AreEqual(true, property.GetValue(repository));
    }
}
