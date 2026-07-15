using EasyAdmin.Application.Dtos;
using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Test;

[TestClass]
public class ShareLifecycleTests
{
    [TestMethod]
    public void ValidateExpiry_RejectsPastOrCurrentTime()
    {
        var now = new DateTime(2026, 7, 15, 8, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ExplicitException>(() => ShareLifecycle.ValidateExpiry(now, now));
        Assert.Throws<ExplicitException>(() => ShareLifecycle.ValidateExpiry(now.AddSeconds(-1), now));
    }

    [TestMethod]
    public void ValidateExpiry_AcceptsPermanentOrFutureTime()
    {
        var now = new DateTime(2026, 7, 15, 8, 0, 0, DateTimeKind.Utc);

        ShareLifecycle.ValidateExpiry(null, now);
        ShareLifecycle.ValidateExpiry(now.AddMinutes(1), now);
    }

    [TestMethod]
    public void ApplyEnabled_ChangesStateAndInvalidatesTokensOnlyWhenValueChanges()
    {
        var share = new ShareEntity { IsEnabled = true, AccessVersion = 2 };

        ShareLifecycle.ApplyEnabled(share, true);
        Assert.AreEqual(2, share.AccessVersion);

        ShareLifecycle.ApplyEnabled(share, false);
        Assert.IsFalse(share.IsEnabled);
        Assert.AreEqual(3, share.AccessVersion);
    }

    [TestMethod]
    public void RegenerateCode_ReplacesCodeAndInvalidatesTokens()
    {
        var share = new ShareEntity { ShareCode = "old", AccessVersion = 2 };

        ShareLifecycle.RegenerateCode(share);

        Assert.AreNotEqual("old", share.ShareCode);
        Assert.AreEqual(64, share.ShareCode.Length);
        Assert.AreEqual(3, share.AccessVersion);
    }
}