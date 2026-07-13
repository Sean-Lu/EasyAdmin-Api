using EasyAdmin.Web.Models;
using EasyAdmin.Web.Services;

namespace EasyAdmin.Test;

[TestClass]
public class OnlineUserSessionAggregatorTests
{
    [TestMethod]
    public void Aggregate_GroupsMultipleSessionsByUserAndKeepsLatestActivity()
    {
        var now = DateTime.UtcNow;
        var records = new[]
        {
            new OnlineUserSessionRecord(7, 11, "10.0.0.1", now.AddMinutes(-20), "Chrome", now.AddMinutes(20)),
            new OnlineUserSessionRecord(7, 11, "10.0.0.2", now.AddMinutes(-5), "Edge", now.AddMinutes(20))
        };

        var result = OnlineUserSessionAggregator.Aggregate(records, now);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result[0].SessionCount);
        Assert.AreEqual("10.0.0.2", result[0].IpAddress);
    }

    [TestMethod]
    public void Aggregate_DoesNotIncludeExpiredRecords()
    {
        var now = DateTime.UtcNow;
        var records = new[]
        {
            new OnlineUserSessionRecord(7, 11, "10.0.0.1", now.AddHours(-2), "Chrome", now.AddMinutes(-1))
        };

        var result = OnlineUserSessionAggregator.Aggregate(records, now);

        Assert.AreEqual(0, result.Count);
    }
}
