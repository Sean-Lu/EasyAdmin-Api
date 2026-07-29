using EasyAdmin.Application.Services;
using EasyAdmin.Infrastructure.Wrapper;
using Microsoft.Extensions.Configuration;

namespace EasyAdmin.Test;

[TestClass]
public class AiApiKeyProtectorTests
{
    [TestMethod]
    public void Encrypt_UsesDifferentNonceAndDecryptsBothValues()
    {
        var protector = CreateProtector("unit-test-jwt-secret");

        var first = protector.Encrypt("sk-secret");
        var second = protector.Encrypt("sk-secret");

        Assert.AreNotEqual(first, second);
        Assert.AreEqual("sk-secret", protector.Decrypt(first));
        Assert.AreEqual("sk-secret", protector.Decrypt(second));
    }

    [TestMethod]
    public void Decrypt_InvalidCiphertextThrowsExplicitException()
    {
        var protector = CreateProtector("unit-test-jwt-secret");

        Assert.Throws<ExplicitException>(() => protector.Decrypt("not-valid-ciphertext"));
    }

    [TestMethod]
    public void Constructor_MissingJwtSecretThrowsExplicitException()
    {
        Assert.Throws<ExplicitException>(() => CreateProtector(null));
    }

    private static AiApiKeyProtector CreateProtector(string? secret)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = secret
            })
            .Build();
        return new AiApiKeyProtector(configuration);
    }
}