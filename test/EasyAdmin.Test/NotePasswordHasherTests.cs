using EasyAdmin.Application.Services;

namespace EasyAdmin.Test;

[TestClass]
public class NotePasswordHasherTests
{
    [TestMethod]
    public void Verify_ReturnsTrue_ForOriginalPassword()
    {
        var result = NotePasswordHasher.Hash("abc12345");
        Assert.IsTrue(NotePasswordHasher.Verify("abc12345", result.Hash, result.Salt));
    }

    [TestMethod]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var result = NotePasswordHasher.Hash("abc12345");
        Assert.IsFalse(NotePasswordHasher.Verify("bad12345", result.Hash, result.Salt));
    }
}
