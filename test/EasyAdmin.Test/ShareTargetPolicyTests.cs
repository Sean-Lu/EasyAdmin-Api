using EasyAdmin.Application.Services;
using EasyAdmin.Domain.Entities;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Test;

[TestClass]
public class ShareTargetPolicyTests
{
    [TestMethod]
    public void CanShareFile_RequiresOwnerTenantAndNormalBizType()
    {
        var file = new FileEntity
        {
            Id = 1,
            TenantId = 2,
            CreateUserId = 7,
            BizType = FileBizType.Normal
        };

        Assert.IsTrue(ShareTargetPolicy.CanShareFile(file, 2, 7));
        Assert.IsFalse(ShareTargetPolicy.CanShareFile(file, 2, 8));

        file.BizType = FileBizType.NoteImage;

        Assert.IsFalse(ShareTargetPolicy.CanShareFile(file, 2, 7));
    }

    [TestMethod]
    public void CanShareFile_RejectsDeletedFile()
    {
        var file = new FileEntity
        {
            Id = 1,
            TenantId = 2,
            CreateUserId = 7,
            BizType = FileBizType.Normal,
            IsDelete = true
        };

        Assert.IsFalse(ShareTargetPolicy.CanShareFile(file, 2, 7));
    }

    [TestMethod]
    public void CanShareNote_RequiresNoteOwnerAndTenant()
    {
        var note = new NoteEntity { Id = 1, TenantId = 2, UserId = 7 };

        Assert.IsTrue(ShareTargetPolicy.CanShareNote(note, 2, 7));
        Assert.IsFalse(ShareTargetPolicy.CanShareNote(note, 3, 7));
        Assert.IsFalse(ShareTargetPolicy.CanShareNote(note, 2, 8));
    }

    [TestMethod]
    public void CanShareNote_RejectsDeletedNote()
    {
        var note = new NoteEntity { Id = 1, TenantId = 2, UserId = 7, IsDelete = true };

        Assert.IsFalse(ShareTargetPolicy.CanShareNote(note, 2, 7));
    }
}