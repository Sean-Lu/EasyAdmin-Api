using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Enums;

namespace EasyAdmin.Test;

[TestClass]
public class FileBizTypeTests
{
    [TestMethod]
    public void CanDeleteFromFileManager_ReturnsTrue_ForNormalFile()
    {
        Assert.IsTrue(FileService.CanDeleteFromFileManager(FileBizType.Normal));
    }

    [TestMethod]
    public void CanDeleteFromFileManager_ReturnsFalse_ForNoteImage()
    {
        Assert.IsFalse(FileService.CanDeleteFromFileManager(FileBizType.NoteImage));
    }

    [TestMethod]
    public void CanDeleteFromFileManager_ReturnsFalse_ForUserAvatar()
    {
        Assert.IsFalse(FileService.CanDeleteFromFileManager(FileBizType.UserAvatar));
    }
}
