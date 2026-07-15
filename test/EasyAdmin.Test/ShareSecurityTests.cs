using EasyAdmin.Application.Services;
using System.Text.RegularExpressions;

namespace EasyAdmin.Test;

[TestClass]
public class ShareSecurityTests
{
    [TestMethod]
    public void CreateCode_ReturnsUnique64CharacterHexValues()
    {
        var first = ShareSecurity.CreateCode();
        var second = ShareSecurity.CreateCode();

        Assert.AreEqual(64, first.Length);
        Assert.IsTrue(Regex.IsMatch(first, "^[0-9a-f]{64}$"));
        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void CalculateTokenExpiry_UsesEarlierOfThirtyMinutesAndShareExpiry()
    {
        var now = new DateTime(2026, 7, 15, 8, 0, 0, DateTimeKind.Utc);

        Assert.AreEqual(now.AddMinutes(30), ShareSecurity.CalculateTokenExpiry(now, null));
        Assert.AreEqual(now.AddMinutes(5), ShareSecurity.CalculateTokenExpiry(now, now.AddMinutes(5)));
    }

    [TestMethod]
    public void IsTokenPayloadValid_RequiresCodeVersionAndFutureExpiry()
    {
        var now = new DateTime(2026, 7, 15, 8, 0, 0, DateTimeKind.Utc);
        var payload = new ShareAccessTokenPayload("share-code", 3, now.AddMinutes(5));

        Assert.IsTrue(ShareSecurity.IsTokenPayloadValid(payload, "share-code", 3, now));
        Assert.IsFalse(ShareSecurity.IsTokenPayloadValid(payload, "other-code", 3, now));
        Assert.IsFalse(ShareSecurity.IsTokenPayloadValid(payload, "share-code", 4, now));
        Assert.IsFalse(ShareSecurity.IsTokenPayloadValid(payload, "share-code", 3, now.AddMinutes(5)));
    }

    [TestMethod]
    public void GetFailureKey_DoesNotExposeClientIp()
    {
        var key = ShareSecurity.GetFailureKey("share-code", "203.0.113.7");

        StringAssert.Contains(key, "share-code");
        Assert.IsFalse(key.Contains("203.0.113.7", StringComparison.Ordinal));
    }
}