using System.Collections.Concurrent;
using System.Text;
using EasyAdmin.Infrastructure.Wrapper;
using EasyAdmin.Web.Contracts;
using EasyAdmin.Web.Models;
using EasyAdmin.Web.Services;
using Microsoft.Extensions.Options;

namespace EasyAdmin.Test;

[TestClass]
public class CaptchaServiceTests
{
    [TestMethod]
    public void SvgGenerator_GeneratesConfiguredLengthAndDataUrl()
    {
        var result = new SvgCaptchaCodeGenerator().Generate(6);

        Assert.AreEqual(6, result.Code.Length);
        Assert.IsTrue(result.Code.All(character => SvgCaptchaCodeGenerator.AllowedCharacters.Contains(character)));
        Assert.IsTrue(result.Image.StartsWith("data:image/svg+xml;base64,", StringComparison.Ordinal));

        var svg = Encoding.UTF8.GetString(Convert.FromBase64String(result.Image.Split(',')[1]));
        StringAssert.Contains(svg, "<svg");
        StringAssert.Contains(svg, "<line");
        StringAssert.Contains(svg, "<circle");
    }

    [TestMethod]
    public async Task GenerateAsync_ReturnsDisabledWithoutUsingDependencies()
    {
        var generator = new FakeCaptchaCodeGenerator();
        var store = new InMemoryCaptchaStore();
        var service = CreateService(false, generator, store);

        var result = await service.GenerateAsync();

        Assert.IsFalse(result.Enabled);
        Assert.IsNull(result.CaptchaKey);
        Assert.IsNull(result.Image);
        Assert.AreEqual(0, generator.GenerateCount);
        Assert.AreEqual(0, store.StoreCount);
    }

    [TestMethod]
    public async Task ValidateAsync_WhenDisabledAllowsLoginWithoutCaptcha()
    {
        var service = CreateService(false, new FakeCaptchaCodeGenerator(), new InMemoryCaptchaStore());

        Assert.IsTrue(await service.ValidateAsync(null, null));
    }

    [TestMethod]
    public void CaptchaOptions_DefaultToDisabledWithPlannedValues()
    {
        var options = new CaptchaOptions();

        Assert.IsFalse(options.Enable);
        Assert.AreEqual(4, options.CodeLength);
        Assert.AreEqual(120, options.ExpireSeconds);
    }

    [TestMethod]
    public void LoginRequest_ProvidesOptionalCaptchaFields()
    {
        var request = new LoginRequest { Account = "admin", Password = "password" };

        Assert.IsNull(request.CaptchaKey);
        Assert.IsNull(request.CaptchaCode);
    }

    [TestMethod]
    public async Task GenerateAsync_WhenEnabledStoresChallengeAndReturnsImage()
    {
        var generator = new FakeCaptchaCodeGenerator();
        var store = new InMemoryCaptchaStore();
        var service = CreateService(true, generator, store);

        var result = await service.GenerateAsync();

        Assert.IsTrue(result.Enabled);
        Assert.IsTrue(Guid.TryParseExact(result.CaptchaKey, "N", out _));
        Assert.AreEqual(FakeCaptchaCodeGenerator.Image, result.Image);
        Assert.AreEqual(1, generator.GenerateCount);
        Assert.AreEqual(1, store.StoreCount);
        Assert.AreEqual(TimeSpan.FromSeconds(120), store.LastExpiration);
    }

    [TestMethod]
    public async Task ValidateAsync_IsCaseInsensitiveAndConsumesChallenge()
    {
        var service = CreateService(true, new FakeCaptchaCodeGenerator(), new InMemoryCaptchaStore());
        var challenge = await service.GenerateAsync();

        Assert.IsTrue(await service.ValidateAsync(challenge.CaptchaKey, "aBcD"));
        Assert.IsFalse(await service.ValidateAsync(challenge.CaptchaKey, "ABCD"));
    }

    [TestMethod]
    public async Task ValidateAsync_WrongCodeConsumesChallenge()
    {
        var service = CreateService(true, new FakeCaptchaCodeGenerator(), new InMemoryCaptchaStore());
        var challenge = await service.GenerateAsync();

        Assert.IsFalse(await service.ValidateAsync(challenge.CaptchaKey, "WRONG"));
        Assert.IsFalse(await service.ValidateAsync(challenge.CaptchaKey, "ABCD"));
    }

    [TestMethod]
    public async Task ValidateAsync_MissingCodeConsumesChallenge()
    {
        var service = CreateService(true, new FakeCaptchaCodeGenerator(), new InMemoryCaptchaStore());
        var challenge = await service.GenerateAsync();

        Assert.IsFalse(await service.ValidateAsync(challenge.CaptchaKey, null));
        Assert.IsFalse(await service.ValidateAsync(challenge.CaptchaKey, "ABCD"));
    }

    [TestMethod]
    public async Task ValidateAsync_RejectsMissingInvalidAndExpiredKeys()
    {
        var store = new InMemoryCaptchaStore();
        var service = CreateService(true, new FakeCaptchaCodeGenerator(), store);
        var challenge = await service.GenerateAsync();
        store.Expire(challenge.CaptchaKey!);

        Assert.IsFalse(await service.ValidateAsync(null, "ABCD"));
        Assert.IsFalse(await service.ValidateAsync("not-a-guid", "ABCD"));
        Assert.IsFalse(await service.ValidateAsync(challenge.CaptchaKey, "ABCD"));
    }

    [TestMethod]
    public async Task ValidateAsync_ConcurrentAttemptsOnlySucceedOnce()
    {
        var service = CreateService(true, new FakeCaptchaCodeGenerator(), new InMemoryCaptchaStore());
        var challenge = await service.GenerateAsync();

        var results = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => service.ValidateAsync(challenge.CaptchaKey, "ABCD")));

        Assert.AreEqual(1, results.Count(result => result));
    }

    [TestMethod]
    public async Task CacheFailure_IsReportedAsExplicitException()
    {
        var service = CreateService(true, new FakeCaptchaCodeGenerator(), new ThrowingCaptchaStore());

        var generateException = await Assert.ThrowsExactlyAsync<ExplicitException>(() => service.GenerateAsync());
        Assert.AreEqual("验证码服务暂不可用", generateException.Message);

        var validateException = await Assert.ThrowsExactlyAsync<ExplicitException>(
            () => service.ValidateAsync(Guid.NewGuid().ToString("N"), "ABCD"));
        Assert.AreEqual("验证码服务暂不可用", validateException.Message);
    }

    private static CaptchaService CreateService(
        bool enable,
        ICaptchaCodeGenerator generator,
        ICaptchaStore store)
    {
        return new CaptchaService(
            Options.Create(new CaptchaOptions
            {
                Enable = enable,
                CodeLength = 4,
                ExpireSeconds = 120
            }),
            generator,
            store);
    }

    private sealed class FakeCaptchaCodeGenerator : ICaptchaCodeGenerator
    {
        public const string Image = "data:image/svg+xml;base64,PHN2Zy8+";
        public int GenerateCount { get; private set; }

        public CaptchaCode Generate(int codeLength)
        {
            GenerateCount++;
            return new CaptchaCode("ABCD", Image);
        }
    }

    private sealed class InMemoryCaptchaStore : ICaptchaStore
    {
        private readonly ConcurrentDictionary<string, string> _values = new();

        public int StoreCount { get; private set; }
        public TimeSpan LastExpiration { get; private set; }

        public Task StoreAsync(string captchaKey, string value, TimeSpan expiration)
        {
            StoreCount++;
            LastExpiration = expiration;
            _values[captchaKey] = value;
            return Task.CompletedTask;
        }

        public Task<string?> ConsumeAsync(string captchaKey)
        {
            _values.TryRemove(captchaKey, out var value);
            return Task.FromResult(value);
        }

        public void Expire(string captchaKey)
        {
            _values.TryRemove(captchaKey, out _);
        }
    }

    private sealed class ThrowingCaptchaStore : ICaptchaStore
    {
        public Task StoreAsync(string captchaKey, string value, TimeSpan expiration)
        {
            throw new InvalidOperationException("cache unavailable");
        }

        public Task<string?> ConsumeAsync(string captchaKey)
        {
            throw new InvalidOperationException("cache unavailable");
        }
    }
}
