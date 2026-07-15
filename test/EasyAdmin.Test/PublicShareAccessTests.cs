using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Test;

[TestClass]
public class PublicShareAccessTests
{
    [TestMethod]
    public void IsUnavailable_RejectsMissingDeletedDisabledAndExpiredShares()
    {
        var now = new DateTime(2026, 7, 15, 8, 0, 0, DateTimeKind.Utc);

        Assert.IsTrue(SharePublicAccess.IsUnavailable(null, now));
        Assert.IsTrue(SharePublicAccess.IsUnavailable(new ShareEntity { Id = 1, IsDelete = true, IsEnabled = true }, now));
        Assert.IsTrue(SharePublicAccess.IsUnavailable(new ShareEntity { Id = 1, IsEnabled = false }, now));
        Assert.IsTrue(SharePublicAccess.IsUnavailable(new ShareEntity { Id = 1, IsEnabled = true, ExpiresAt = now }, now));
        Assert.IsFalse(SharePublicAccess.IsUnavailable(new ShareEntity { Id = 1, IsEnabled = true, ExpiresAt = now.AddSeconds(1) }, now));
        Assert.IsFalse(SharePublicAccess.IsUnavailable(new ShareEntity { Id = 1, IsEnabled = true }, now));
    }

    [TestMethod]
    public void CanAccessNoteImage_RequiresCurrentReferenceAndMatchingOwner()
    {
        var note = new NoteEntity { Id = 9, TenantId = 2, UserId = 7 };
        var image = new FileEntity
        {
            Id = 12,
            TenantId = 2,
            CreateUserId = 7,
            BizType = FileBizType.NoteImage
        };

        Assert.IsTrue(SharePublicAccess.CanAccessNoteImage(note, image, new HashSet<long> { 12 }));
        Assert.IsFalse(SharePublicAccess.CanAccessNoteImage(note, image, new HashSet<long> { 13 }));

        image.CreateUserId = 8;

        Assert.IsFalse(SharePublicAccess.CanAccessNoteImage(note, image, new HashSet<long> { 12 }));
    }

    [TestMethod]
    public void CanAccessNoteImage_RequiresNoteImageBizType()
    {
        var note = new NoteEntity { Id = 9, TenantId = 2, UserId = 7 };
        var image = new FileEntity
        {
            Id = 12,
            TenantId = 2,
            CreateUserId = 7,
            BizType = FileBizType.Normal
        };

        Assert.IsFalse(SharePublicAccess.CanAccessNoteImage(note, image, new HashSet<long> { 12 }));
    }
}