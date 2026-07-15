using EasyAdmin.Web.Models;

namespace EasyAdmin.Test;

[TestClass]
public class SingleTokenSessionTests
{
    [TestMethod]
    public void ToOnlineSessionRecord_PreservesSessionMetadata()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-5);
        var expiresAt = DateTime.UtcNow.AddMinutes(55);
        var session = new SingleTokenSessionModel
        {
            Token = "token",
            UserId = 7,
            TenantId = 11,
            IpAddress = "192.168.1.20",
            UserAgent = "Mozilla/5.0 Chrome",
            CreatedAt = createdAt,
            ExpiresAt = expiresAt
        };

        var record = SingleTokenSessionMapper.ToOnlineSessionRecord(session);

        Assert.AreEqual(session.UserId, record.UserId);
        Assert.AreEqual(session.TenantId, record.TenantId);
        Assert.AreEqual(session.IpAddress, record.IpAddress);
        Assert.AreEqual(session.UserAgent, record.UserAgent);
        Assert.AreEqual(session.CreatedAt, record.LoginTime);
        Assert.AreEqual(session.ExpiresAt, record.ExpiresAt);
    }
}
