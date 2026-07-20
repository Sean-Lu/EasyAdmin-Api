using System.Security.Cryptography;
using System.Text;
using EasyAdmin.Infrastructure.Const;
using EasyAdmin.Infrastructure.Enums;
using EasyAdmin.Infrastructure.Models;
using EasyAdmin.Web.Models;
using EasyAdmin.Web.Services;
using Microsoft.IdentityModel.Tokens;
using Sean.Core.Redis;

namespace EasyAdmin.Test;

[TestClass]
public class TokenServiceTests
{
    private const long UserId = 7001;
    private string? rawToken;

    [TestInitialize]
    public void Initialize() => RedisTestSetup.EnsureInitialized();

    [TestCleanup]
    public async Task Cleanup()
    {
        await RedisHelper.KeyDeleteAsync($"{CacheKeyConst.TokenPrefix}{UserId}");
        if (rawToken is not null)
        {
            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken)));
            await RedisHelper.KeyDeleteAsync($"{CacheKeyConst.TokenBlacklistPrefix}{hash}");
        }
    }

    [TestMethod]
    public async Task RevokeUserSessionsAsync_AddsStoredSingleTokenToBlacklist()
    {
        var config = new JwtConfig
        {
            SecretKey = "test-secret-key-which-is-long-enough",
            Issuer = "test-issuer",
            Audience = "test-audience",
            TokenMode = TokenMode.Single,
            Expired = 30
        };
        var service = new TokenService(config);
        var token = service.GenerateAccessToken(new JwtUserModel { UserId = UserId, TenantId = 1 });
        rawToken = token["Bearer ".Length..];

        await service.StoreSingleTokenSessionAsync(
            new JwtUserModel { UserId = UserId, TenantId = 1 },
            rawToken,
            "127.0.0.1",
            "test-agent");

        await service.RevokeUserSessionsAsync(UserId, "管理员强制下线");

        Assert.IsTrue(await service.IsTokenBlacklistedAsync(rawToken));
        Assert.IsNull(await service.GetSingleTokenSessionAsync(UserId));
    }
}
