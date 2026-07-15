using EasyAdmin.Application.Services;
using Microsoft.Extensions.Configuration;

namespace EasyAdmin.Test;

[TestClass]
public class SharePasswordProtectorTests
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "test-secret-key-for-share-password-encryption"
        })
        .Build();

    [TestMethod]
    public void Encrypt_StoresCiphertextAndDecryptsOriginalPassword()
    {
        var protector = new SharePasswordProtector(Configuration);

        var ciphertext = protector.Encrypt("note-share-123");

        Assert.AreNotEqual("note-share-123", ciphertext);
        Assert.AreEqual("note-share-123", protector.Decrypt(ciphertext));
    }

    [TestMethod]
    public void IsMatch_OnlyMatchesOriginalPassword()
    {
        var protector = new SharePasswordProtector(Configuration);
        var ciphertext = protector.Encrypt("file-share-123");

        Assert.IsTrue(protector.IsMatch("file-share-123", ciphertext));
        Assert.IsFalse(protector.IsMatch("wrong-password", ciphertext));
    }
}