using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;

namespace EasyAdmin.Test;

[TestClass]
public class ShareListTests
{
    [TestMethod]
    public void GetListStatus_UsesTargetAvailabilityBeforeShareLifecycle()
    {
        var now = new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc);

        Assert.AreEqual(ShareListStatus.Normal, ShareLifecycle.GetListStatus(true, now.AddHours(1), true, now));
        Assert.AreEqual(ShareListStatus.Disabled, ShareLifecycle.GetListStatus(false, now.AddHours(1), true, now));
        Assert.AreEqual(ShareListStatus.Expired, ShareLifecycle.GetListStatus(true, now.AddMinutes(-1), true, now));
        Assert.AreEqual(ShareListStatus.TargetDeleted, ShareLifecycle.GetListStatus(true, now.AddHours(1), false, now));
    }
}
