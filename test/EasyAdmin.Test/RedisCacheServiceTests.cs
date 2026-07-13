using EasyAdmin.Application.Services;

namespace EasyAdmin.Test;

[TestClass]
public class RedisCacheServiceTests
{
    [TestMethod]
    public void SortKeys_UsesOrdinalOrder()
    {
        var keys = new[]
        {
            "EasyAdmin:TokenBlacklist:x",
            "EasyAdmin:Token:2",
            "EasyAdmin:Token:10"
        };

        var sorted = RedisCacheService.SortKeys(keys);

        CollectionAssert.AreEqual(
            new[]
            {
                "EasyAdmin:Token:10",
                "EasyAdmin:Token:2",
                "EasyAdmin:TokenBlacklist:x"
            },
            sorted);
        CollectionAssert.AreEqual(sorted, RedisCacheService.SortKeys(sorted));
    }

    [TestMethod]
    public void NormalizeDatabase_RejectsIndexesOutsideConfiguredRange()
    {
        Assert.AreEqual(0, RedisCacheService.NormalizeDatabase(0, 4));
        Assert.AreEqual(3, RedisCacheService.NormalizeDatabase(3, 4));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => RedisCacheService.NormalizeDatabase(-1, 4));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => RedisCacheService.NormalizeDatabase(4, 4));
    }
}
