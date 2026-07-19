using EasyAdmin.Application.Services;
using EasyAdmin.Application.Dtos;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Wrapper;

namespace EasyAdmin.Test;

[TestClass]
public class FavoriteRulesTests
{
    [TestMethod]
    public void IsCollectibleMenu_RequiresEnabledOpenableItem()
    {
        Assert.IsTrue(FavoriteRules.IsCollectibleMenu(new MenuEntity
        {
            Id = 1,
            Type = MenuType.Internal,
            State = CommonState.Enable,
            Path = "/user/note",
            Children = new List<MenuEntity>()
        }));
        Assert.IsTrue(FavoriteRules.IsCollectibleMenu(new MenuEntity
        {
            Id = 2,
            Type = MenuType.External,
            State = CommonState.Enable,
            Path = "/link",
            OutLink = "https://example.com",
            Children = new List<MenuEntity> { new() { Id = 3 } }
        }));
        Assert.IsFalse(FavoriteRules.IsCollectibleMenu(new MenuEntity
        {
            Id = 4,
            Type = MenuType.Directory,
            State = CommonState.Enable,
            Path = "/user",
            Children = new List<MenuEntity> { new() { Id = 5 } }
        }));
        Assert.IsFalse(FavoriteRules.IsCollectibleMenu(new MenuEntity
        {
            Id = 6,
            Type = MenuType.Internal,
            State = CommonState.Disable,
            Path = "/disabled",
            Children = new List<MenuEntity>()
        }));
        Assert.IsFalse(FavoriteRules.IsCollectibleMenu(new MenuEntity
        {
            Id = 7,
            Type = MenuType.Internal,
            State = CommonState.Enable,
            Children = new List<MenuEntity>()
        }));
        Assert.IsFalse(FavoriteRules.IsCollectibleMenu(new MenuEntity
        {
            Id = 8,
            Type = MenuType.Directory,
            State = CommonState.Enable,
            Path = "/not-a-page",
            Children = new List<MenuEntity>()
        }));
    }

    [TestMethod]
    public void GetShareStatus_PreservesEachInvalidReason()
    {
        var now = new DateTime(2026, 7, 17, 8, 0, 0, DateTimeKind.Utc);

        Assert.AreEqual(FavoriteAvailabilityStatus.Normal,
            FavoriteRules.GetShareStatus(new ShareEntity { Id = 1, IsEnabled = true }, true, now));
        Assert.AreEqual(FavoriteAvailabilityStatus.ShareDisabled,
            FavoriteRules.GetShareStatus(new ShareEntity { Id = 1, IsEnabled = false }, true, now));
        Assert.AreEqual(FavoriteAvailabilityStatus.ShareExpired,
            FavoriteRules.GetShareStatus(new ShareEntity { Id = 1, IsEnabled = true, ExpiresAt = now }, true, now));
        Assert.AreEqual(FavoriteAvailabilityStatus.ShareTargetDeleted,
            FavoriteRules.GetShareStatus(new ShareEntity { Id = 1, IsEnabled = true }, false, now));
    }

    [TestMethod]
    public void MapShareTargetType_MapsOnlyFileAndNote()
    {
        Assert.AreEqual(FavoriteTargetType.File, FavoriteRules.MapShareTargetType(ShareTargetType.File));
        Assert.AreEqual(FavoriteTargetType.Note, FavoriteRules.MapShareTargetType(ShareTargetType.Note));
    }

    [TestMethod]
    public void ValidateStatusRequest_RejectsMixedOrOversizedRequests()
    {
        var mixed = new FavoriteStatusReqDto
        {
            ShareCode = new string('a', 64),
            Targets = new List<FavoriteTargetReqDto>
            {
                new() { TargetType = FavoriteTargetType.Menu, TargetId = 1 }
            }
        };
        Assert.Throws<ExplicitException>(() => FavoriteRules.ValidateStatusRequest(mixed));

        var oversized = new FavoriteStatusReqDto
        {
            Targets = Enumerable.Range(1, 201)
                .Select(id => new FavoriteTargetReqDto { TargetType = FavoriteTargetType.File, TargetId = id })
                .ToList()
        };
        Assert.Throws<ExplicitException>(() => FavoriteRules.ValidateStatusRequest(oversized));
    }

    [TestMethod]
    public void ResolveShareDisplay_UsesSnapshotOnlyWhenUnavailable()
    {
        var live = FavoriteRules.ResolveShareDisplay(
            FavoriteAvailabilityStatus.Normal,
            "实时标题",
            "实时分享者",
            "快照标题",
            "快照分享者");
        Assert.AreEqual("实时标题", live.Title);
        Assert.AreEqual("实时分享者", live.OwnerName);

        var invalid = FavoriteRules.ResolveShareDisplay(
            FavoriteAvailabilityStatus.ShareExpired,
            null,
            null,
            "快照标题",
            "快照分享者");
        Assert.AreEqual("快照标题", invalid.Title);
        Assert.AreEqual("快照分享者", invalid.OwnerName);
    }
}
