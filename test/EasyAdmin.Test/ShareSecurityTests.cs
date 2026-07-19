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
}