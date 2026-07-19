using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Test;

[TestClass]
public class TenantValidityTests
{
    [TestMethod]
    public void IsTenantValid_UsesHalfOpenValidityInterval()
    {
        var now = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

        Assert.IsTrue(TenantAccessPolicy.IsTenantValid(CommonState.Enable, null, null, now));
        Assert.IsFalse(TenantAccessPolicy.IsTenantValid(CommonState.Enable, now.AddSeconds(1), null, now));
        Assert.IsTrue(TenantAccessPolicy.IsTenantValid(CommonState.Enable, null, now.AddSeconds(1), now));
        Assert.IsFalse(TenantAccessPolicy.IsTenantValid(CommonState.Enable, null, now, now));
        Assert.IsFalse(TenantAccessPolicy.IsTenantValid(CommonState.Disable, null, null, now));
    }

    [TestMethod]
    public void GetCacheDuration_DoesNotPassNextValidityBoundary()
    {
        var now = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

        Assert.AreEqual(TimeSpan.FromSeconds(10), TenantAccessPolicy.GetCacheDuration(now, now.AddSeconds(10)));
        Assert.AreEqual(TimeSpan.FromSeconds(30), TenantAccessPolicy.GetCacheDuration(now, now.AddMinutes(1)));
        Assert.AreEqual(TimeSpan.FromSeconds(30), TenantAccessPolicy.GetCacheDuration(now, null));
    }
}
